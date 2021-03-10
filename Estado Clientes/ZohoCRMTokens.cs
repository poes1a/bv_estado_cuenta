using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;


namespace CronClientes
{
    public class ZohoCRMTokens
    {
        public String AccessToken()
        {
            var details = JObject.Parse("{}");
            var endPoint = "https://accounts.zoho.com";
            var client_id = "1000.1512ROYWHWTP205320NX2GYKGKT7KU";
            var client_secret = "f53a8792401865bc7c7a87c565dfc3937fced9b100";
            var refresh_token = "1000.3f0e6d46b04fdde8b883a475aecbc0f1.249bbb0c381a38de669a43ee4dd3070d";
            var client = new RestClient(endPoint + "/oauth/v2/token?");
            var request = new RestRequest(Method.POST);
            request.AddParameter("refresh_token", refresh_token);
            request.AddParameter("client_id", client_id);
            request.AddParameter("client_secret", client_secret);
            request.AddParameter("grant_type", "refresh_token");
            try
            {
                IRestResponse response = client.Execute(request);
                var responseToken = response.Content;
                details = JObject.Parse(responseToken);
                return details["access_token"].ToString();
            }
            catch (Exception e)
            {
                var error = details.ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
                Console.WriteLine("ERROR : " + DateTime.Now.ToString("HH:mm:ss") + " Problema con access token " + error + ", Exception : " + e);
                return "ERROR";
            }


        }
    }
}
