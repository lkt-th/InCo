using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InCo.Network
{
    public interface IHttpServiceAsync
    {
        Task<T> GetAsync<T>(string url);
        Task<T> PostAsync<T>(string url, object data);
        Task<T> PostFormAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> data);
        Task<T> PostFileAsync<T>(string url, List<string> files);
        Task<T> PutAsync<T>(string url, object data);
    }

    public class HttpServiceAsync : IHttpServiceAsync
    {
        private readonly HttpClient _httpClient;
        private readonly bool _ignoreCertificateValidation;
        private readonly JsonSerializerSettings _serializerSettings;

        public HttpServiceAsync(Uri host, JsonSerializerSettings settings = null, bool ignoreCertificateValidation = false)
        {
            if (settings != null)
                _serializerSettings = settings;
            else
                _serializerSettings = new JsonSerializerSettings();

            var clientHandler = new HttpClientHandler() { UseCookies = true };
            clientHandler.CookieContainer = new CookieContainer();

            _httpClient = new HttpClient(clientHandler) { BaseAddress = host };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("InCo/0.1");

            _ignoreCertificateValidation = ignoreCertificateValidation;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();

            // Ignore Ssl validation.
            if (_ignoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Add TLS 1.2 Supported.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                source.SetResult(JsonConvert.DeserializeObject<T>(content));
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                source.SetException(new Exception(response.ReasonPhrase));
            }

            return await source.Task;
        }

        public async Task<T> PostAsync<T>(string url, object data)
        {
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();
            try
            {
                string json = JsonConvert.SerializeObject(data, _serializerSettings);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                if (data == null) content = null;

                // Ignore Ssl validation.
                if (_ignoreCertificateValidation)
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                // Add TLS 1.2 Supported.
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    source.SetResult(JsonConvert.DeserializeObject<T>(responseContent));
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    source.SetException(new Exception(response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                source.SetException(ex);
            }

            return await source.Task;
        }

        public async Task<T> PostFileAsync<T>(string url, List<string> files)
        {
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();
            MultipartFormDataContent multipart = new MultipartFormDataContent();
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    string fileName = Path.GetFileName(file);
                    var fs = File.OpenRead(file);
                    StreamContent content = new StreamContent(fs);
                    multipart.Add(content, "File", fileName);
                }
                else
                {
                    throw new FileNotFoundException("File not exist.",file);
                }    
            }

            try
            {
                // Ignore Ssl validation.
                if (_ignoreCertificateValidation)
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                // Add TLS 1.2 Supported.
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                var response = await _httpClient.PostAsync(url, multipart);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    source.SetResult(JsonConvert.DeserializeObject<T>(responseContent));
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    source.SetException(new Exception(response.ReasonPhrase));
                }
            }
            catch(Exception ex)
            {
                source.SetException(ex);
            }

            return await source.Task;
        }

        public async Task<T> PostFormAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> data)
        {
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();

            FormUrlEncodedContent content = new FormUrlEncodedContent(data);

            // Ignore Ssl validation.
            if (_ignoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Add TLS 1.2 Supported.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                source.SetResult(JsonConvert.DeserializeObject<T>(responseContent));
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<T>(responseContent);
                    if (result != null) source.SetResult(result);
                    else
                        throw new JsonException($"Cannot parse {responseContent}");
                }
                catch
                {
                    source.SetException(new Exception(response.ReasonPhrase));
                }
            }

            return await source.Task;
        }

        public async Task<T> PutAsync<T>(string url, object data)
        {
            TaskCompletionSource<T> source = new TaskCompletionSource<T>();

            try
            {
                string json = JsonConvert.SerializeObject(data, _serializerSettings);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                if (data == null) content = null;

                // Ignore Ssl validation.
                if (_ignoreCertificateValidation)
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                // Add TLS 1.2 Supported.
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                var response = await _httpClient.PutAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    source.SetResult(JsonConvert.DeserializeObject<T>(responseContent));
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    source.SetException(new Exception(response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                source.SetException(ex);
            }

            return await source.Task;
        }
    }
}
