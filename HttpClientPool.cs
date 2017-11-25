using System.Collections.Generic;
using System.Threading;

namespace System.Net.Http {
	/// <summary>
	/// HttpClient套字接池
	/// </summary>
	public partial class HttpClientPool {

		private static HttpClientPool _Instance;
		public static HttpClientPool Instance => _Instance ?? (_Instance = new HttpClientPool(50));

		public int MaxPoolSize = 32;
		public List<HttpClientBag> AllHttpClients = new List<HttpClientBag>();
		public Queue<HttpClientBag> FreeHttpClients = new Queue<HttpClientBag>();
		public Queue<ManualResetEvent> GetHttpClientQueue = new Queue<ManualResetEvent>();
		private static object _lock = new object();
		private static object _lock_GetHttpClientQueue = new object();

		public HttpClientPool (int maxPoolSize) {
			MaxPoolSize = maxPoolSize;
		}

		public HttpClientBag GetHttpClient () {
			HttpClientBag bag = null;
			if (FreeHttpClients.Count > 0)
				lock (_lock)
					if (FreeHttpClients.Count > 0)
						bag = FreeHttpClients.Dequeue();
			if (bag == null && AllHttpClients.Count < MaxPoolSize) {
				lock (_lock)
					if (AllHttpClients.Count < MaxPoolSize) {
						bag = new HttpClientBag();
						AllHttpClients.Add(bag);
					}
				if (bag != null) {
					bag.Pool = this;
					bag.HttpClient = new HttpClient();
				}
			}
			if (bag == null) {
				ManualResetEvent wait = new ManualResetEvent(false);
				lock (_lock_GetHttpClientQueue)
					GetHttpClientQueue.Enqueue(wait);
				if (wait.WaitOne(TimeSpan.FromSeconds(10)))
					return GetHttpClient();
				return null;
			}
			bag.ThreadId = Thread.CurrentThread.ManagedThreadId;
			bag.LastActive = DateTime.Now;
			Interlocked.Increment(ref bag.UseSum);
			return bag;
		}

		public void ReleaseHttpClient (HttpClientBag bag) {
			lock (_lock)
				FreeHttpClients.Enqueue(bag);

			if (GetHttpClientQueue.Count > 0) {
				ManualResetEvent wait = null;
				lock (_lock_GetHttpClientQueue)
					if (GetHttpClientQueue.Count > 0)
						wait = GetHttpClientQueue.Dequeue();
				if (wait != null) wait.Set();
			}
		}
	}

	public class HttpClientBag : IDisposable {
		public HttpClient HttpClient;
		public DateTime LastActive;
		public long UseSum;
		internal int ThreadId;
		internal HttpClientPool Pool;

		public void Dispose () {
			if (Pool != null) Pool.ReleaseHttpClient(this);
		}
	}
}