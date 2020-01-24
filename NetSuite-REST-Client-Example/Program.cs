using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace NetSuiteRESTExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = ServiceProviderBuilder.GetServiceProvider(args);
            var nsAuth = services.GetService<INetSuiteOAuth>();

            string accountId = nsAuth.AccountId;
            string baseUrl = string.Format("https://{0}.suitetalk.api.netsuite.com/rest/platform/v1/record/", accountId);

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.oracle.resource+json"));

            // Simple GET
            //
            using (var httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Get;
                httpRequest.RequestUri = new Uri(string.Concat(baseUrl, "job/3897"));

                httpRequest.Headers.Add("Authorization", nsAuth.GetAuthorizationHeader("GET", httpRequest.RequestUri));

                var httpResponse = httpClient.SendAsync(httpRequest).Result;
                httpResponse.EnsureSuccessStatusCode();

                var responseContent = httpResponse.Content.ReadAsStringAsync().Result;

                //Debugger.Break();
            }

            // GET with Query Parameters
            //
            using (var httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Get;
                httpRequest.RequestUri = new Uri(string.Concat(baseUrl, "job/3897?expandSubResources=true"));

                httpRequest.Headers.Add("Authorization", nsAuth.GetAuthorizationHeader("GET", httpRequest.RequestUri, new Dictionary<string, string>() { { "expandSubResources", "true" } }));

                var httpResponse = httpClient.SendAsync(httpRequest).Result;
                httpResponse.EnsureSuccessStatusCode();

                var responseContent = httpResponse.Content.ReadAsStringAsync().Result;

                //Debugger.Break();
            }

            // POST
            //
            using (var httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = new Uri(string.Concat(baseUrl, "customer"));

                var json = JsonConvert.SerializeObject(new { companyname = "Company 123" });
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                httpRequest.Content = stringContent;

                httpRequest.Headers.Add("Authorization", nsAuth.GetAuthorizationHeader("POST", httpRequest.RequestUri));

                var httpResponse = httpClient.SendAsync(httpRequest).Result;
                httpResponse.EnsureSuccessStatusCode();

                var responseContent = httpResponse.Content.ReadAsStringAsync().Result;

                //Debugger.Break();
            }
        }
    }
}
