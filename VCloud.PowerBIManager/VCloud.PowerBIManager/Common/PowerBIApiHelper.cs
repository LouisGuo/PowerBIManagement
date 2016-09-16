using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VCloud.PowerBIManager
{
    public class PowerBIApiHelper
    {
        private readonly String accessToken;
        private readonly String subscriptionId;
        private readonly String version = "2016-01-29";
        private readonly String azureEndpointUri = "https://management.azure.com";

        public PowerBIApiHelper(String accessToken, String subscriptionId)
        {
            this.accessToken = accessToken;
            this.subscriptionId = subscriptionId;
        }

        public String ListWorkspaceCollectionKey(String resourceGroup, String workspaceCollectionName)
        {
            var url = String.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.PowerBI/workspaceCollections/{3}/listkeys/?api-version={4}", azureEndpointUri, subscriptionId, resourceGroup, workspaceCollectionName, version);
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                // Set authorization header from you acquired Azure AD token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.accessToken);
                request.Content = new StringContent(String.Empty);
                var response = client.SendAsync(request).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var responseText = response.Content.ReadAsStringAsync();
                    var message = String.Format("Status: {0}, Reason: {1}, Message: {2}", response.StatusCode, response.ReasonPhrase, responseText);
                    throw new Exception(message);
                }
                var json = response.Content.ReadAsStringAsync().Result;
                return JsonSerializer.ConvertStringToObj<JObject>(json)["key1"].ToString();
            }
        }

        public IEnumerable<Tuple<String, String>> GetWorkspaceCollections()
        {
            var url = String.Format("{0}/subscriptions/{1}/providers/Microsoft.PowerBI/workspaceCollections/?api-version={2}", azureEndpointUri, subscriptionId, version);
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.accessToken);
                var response = client.SendAsync(request).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var responseText = response.Content.ReadAsStringAsync();
                    var message = String.Format("Status: {0}, Reason: {1}, Message: {2}", response.StatusCode, response.ReasonPhrase, responseText);
                    throw new Exception(message);
                }
                var json = response.Content.ReadAsStringAsync().Result;
                var collections = JsonSerializer.ConvertStringToObj<JObject>(json)["value"] as JArray;
                return collections.Select(c => Tuple.Create(c["id"].Value<String>().Split('/')[4], c["name"].Value<String>()));
            }
        }
    }
}
