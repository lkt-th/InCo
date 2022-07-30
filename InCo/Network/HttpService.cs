using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InCo.Network
{
    public interface IHttpService
    {
        T Get<T>(string url);
        T Post<T>(string url, object data);
        T PostForm<T>(string url, IEnumerable<KeyValuePair<string, string>> data);
        T PostFile<T>(string url, List<string> files);
        T Put<T>(string url, object data);
    }

    public class HttpService : IHttpService
    {
        private readonly IHttpServiceAsync httpService;
        public HttpService(Uri host, JsonSerializerSettings settings = null)
        {
            httpService = new HttpServiceAsync(host, settings);
        }

        public T Get<T>(string url)
        {
            return httpService.GetAsync<T>(url).Result;
        }

        public T Post<T>(string url, object data)
        {
            return httpService.PostAsync<T>(url, data).Result;
        }

        public T PostFile<T>(string url, List<string> files)
        {
            return httpService.PostFileAsync<T>(url, files).Result;
        }

        public T PostForm<T>(string url, IEnumerable<KeyValuePair<string, string>> data)
        {
            return httpService.PostFormAsync<T>(url, data).Result;
        }

        public T Put<T>(string url, object data)
        {
            return httpService.PutAsync<T>(url, data).Result;
        }
    }
}
