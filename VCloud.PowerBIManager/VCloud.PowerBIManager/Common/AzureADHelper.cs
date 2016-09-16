using Microsoft.IdentityModel.Clients.ActiveDirectory;
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
    public class AzureADHelper
    {

        private static readonly String armResource = "https://management.core.windows.net/";
        private static readonly String clientId = "ea0616ba-638b-4df5-95b9-636659ae5121";
        private static readonly Uri redirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");

        public static String GetAzureAccessToken(AuthenticationResult commonToken)
        {
            var tenantId = (GetTenantIds(commonToken.AccessToken)).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(tenantId))
            {
                throw new InvalidOperationException("Unable to get tenant id for user accout");
            }
            var authority = String.Format("https://login.windows.net/{0}/oauth2/authorize", tenantId);
            var authContext = new AuthenticationContext(authority);
            var result = authContext.AcquireTokenByRefreshToken(commonToken.RefreshToken, clientId, armResource);
            return result.AccessToken;
        }

        public static AuthenticationResult GetCommonAzureAccessToken()
        {
            var authContext = new AuthenticationContext("https://login.windows.net/common/oauth2/authorize");
            var result = authContext.AcquireToken(
                resource: armResource,
                clientId: clientId,
                redirectUri: redirectUri,
                promptBehavior: PromptBehavior.Auto);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            return result;
        }

        private static IEnumerable<String> GetTenantIds(String commonToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + commonToken);
                var response = httpClient.GetStringAsync("https://management.azure.com/tenants?api-version=2016-01-29").Result;
                var tenantsJson = JsonSerializer.ConvertStringToObj<JObject>(response);
                var tenants = tenantsJson["value"] as JArray;
                return tenants.Select(t => t["tenantId"].Value<String>());
            }
        }

        public static String GetSubscriptionId(AuthenticationResult azureCommonToken)
        {
            using (var httpClient = new HttpClient())
            {
                var commonToken = azureCommonToken.AccessToken;
                httpClient.DefaultRequestHeaders.Add("x-ms-version", "2013-08-01");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + commonToken);
                var response = httpClient.GetStringAsync("https://management.core.windows.net/subscriptions").Result;
                var result = String.Empty;
                if (response != null && !response.Equals(String.Empty))
                {
                    var startIndex = response.IndexOf("<SubscriptionID>");
                    var endIndex = response.IndexOf("</SubscriptionID>");
                    result = response.Substring(startIndex + 16, endIndex - startIndex - 16);
                }
                return result;
            }
        }

    }
}
