### HttpClientPool

```html

.NET HttpClient的缺陷和文档错误让开发人员倍感沮丧

https://news.cnblogs.com/n/553217/

```

只能说.net core没有包装一个更高级的类给大家用

结果大家只用底层的类，又没有控制资源导致报错

HttpClient类要自己写连接池管理，不能一直 new

### 使用方式，再不会出报错

```c#
static string HttpGet(string url)
{
    using (var bag = HttpClientPool.Instance.GetHttpClient()) {
        var t = bag.HttpClient.GetStringAsync(url);
        return t.Result;
    } 
}
```