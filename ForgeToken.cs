
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ForgeFunction
{
    public static class ForgeToken
    {
        [FunctionName("ForgeToken")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string client_id = GetEnvironmentVariable("FORGE_CLIENT_ID");
            string client_secret = GetEnvironmentVariable("FORGE_CLIENT_SECRET");

            AccessToken accessToken = GetForgeToken(client_id, client_secret).Result;

            return (ActionResult)new OkObjectResult(accessToken);
        }

        private static async Task<AccessToken> GetForgeToken(string client_id, string client_secret)
        {
            string uri = "https://developer.api.autodesk.com/authentication/v1/authenticate";

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            List<KeyValuePair<string, string>> contentValues = new List<KeyValuePair<string, string>>();
            contentValues.Add(new KeyValuePair<string, string>("client_id", client_id));
            contentValues.Add(new KeyValuePair<string, string>("client_secret", client_secret));
            contentValues.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            contentValues.Add(new KeyValuePair<string, string>("scope", "data:write viewables:read data:read bucket:read bucket:delete"));

            FormUrlEncodedContent formContent = new FormUrlEncodedContent(contentValues);

            HttpResponseMessage response = await client.PostAsync(uri, formContent);

            // Deserialize the access token from the response body.
            AccessToken acessToken = await response.Content.ReadAsAsync<AccessToken>();
            return acessToken;
        }

        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, System.EnvironmentVariableTarget.Process);
        }
    }


    public class AccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
}
