using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;

namespace testhttp {
	class Program {
		static string HttpGet (string url) {
			using (var bag = HttpClientPool.Instance.GetHttpClient()) {
				var t = bag.HttpClient.GetStringAsync(url);
				return t.Result;
			}
		}

		static void Main (string[] args) {
			DateTime dt = DateTime.Now;
			long counter_success = 0;
			long counter_error = 0;
			var running = true;
			for (var a = 0; a < 100; a++) {
				new Thread((obja) => {
					while (running) {
						var dt2 = DateTime.Now;
						try {
							var value = HttpGet("http://www.duoyi.com/dycommerce/ch/");
							Console.WriteLine($"{obja}: {value.Substring(0, 20)} {DateTime.Now.Subtract(dt2).TotalMilliseconds}ms {Interlocked.Increment(ref counter_success)}次成功");
						} catch (Exception ex) {
							Console.WriteLine($"{obja}: {ex.Message} {DateTime.Now.Subtract(dt2).TotalMilliseconds}ms {Interlocked.Increment(ref counter_error)}次错误");
						}
					}
				}).Start(a);
			}
			Console.WriteLine("1111");
			while (Console.ReadKey().Key != ConsoleKey.Escape) {
				var dt3 = DateTime.Now;
				var value2 = HttpGet("http://www.duoyi.com/dycommerce/ch/");
				Console.WriteLine($"{value2.Substring(0, 20)} {DateTime.Now.Subtract(dt3).TotalMilliseconds}ms");
			}
			running = false;

			var ts = DateTime.Now.Subtract(dt).TotalSeconds;
			Console.WriteLine($"总耗时：{ts}秒");
			Console.WriteLine($"共成功读取{counter_success}次，平均{counter_success / ts}/秒错误");
			Console.WriteLine($"共错误{counter_error}次，平均{counter_error / ts}/秒错误");
		}
	}
}