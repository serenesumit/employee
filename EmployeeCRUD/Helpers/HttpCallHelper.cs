using Core;
using Core.Helpers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Helpers
{
    public class HttpCallHelper : IHttpCallHelper
    {
        public string Get(string url)
        {
            var response = this.ReadResponseAsString(Method.GET, url, string.Empty);
            return response;
        }

        public async Task<string> GetAsync(string url)
        {
            var responseString = string.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    responseString = await response.Content.ReadAsStringAsync();
                }
            }

            return responseString;
        }

        public string Post(string url, string postData)
        {
            var response = this.ReadResponseAsString(Method.POST, url, postData);
            return response;
        }

        public async Task<string> PostAsync(string url, string postData)
        {
            var responseString = string.Empty;
            using (var client = new HttpClient())
            {
                ////  client.DefaultRequestHeaders.Add("content-type", "application/x-www-form-urlencoded");
                var content = new StringContent(postData);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    responseString = await response.Content.ReadAsStringAsync();
                }
            }

            return responseString;
        }

        public string ReadResponseAsString(Method method, string url, string postData)
        {
            HttpWebRequest webRequest = null;
            string responseData = string.Empty;

            webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method.ToString();
            webRequest.ServicePoint.Expect100Continue = false;

            if (method == Method.POST)
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";

                // POST the data.
                using (var requestWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    requestWriter.Write(postData);
                }
            }

            using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
            {
                responseData = responseReader.ReadToEnd();
            }

            return responseData;
        }

        public async Task<string> ReadResponseAsStringAsync(Method method, string url, string postData)
        {
            HttpWebRequest webRequest = null;
            string responseData = string.Empty;

            webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method.ToString();
            webRequest.ServicePoint.Expect100Continue = false;

            if (method == Method.POST)
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";

                // POST the data.
                using (var requestWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    await requestWriter.WriteAsync(postData);
                }
            }

            using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
            {
                responseData = await responseReader.ReadToEndAsync();
            }

            return responseData;
        }

    
    }
}
