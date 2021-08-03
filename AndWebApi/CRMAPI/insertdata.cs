using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Web;

namespace AndWebApi.CRMAPI
{
    public class insertdata
    {

        public string gettoken()
        {
            string crm_token = "";

            var token_client = new RestClient("http://192.168.2.4:88/token");

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            var token_request = new RestRequest("/cerberus/connect/token", Method.POST);

            token_request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            token_request.AddHeader("Accept", "application/json");
            //token_request.AddParameter("grant_type", "password");
            //token_request.AddParameter("username", "NodibSoftwares");
            //token_request.AddParameter("password", "N@d!b$of6war35");
           
            

          
            var token_response = token_client.Post(token_request);

            if (token_response.StatusCode == HttpStatusCode.OK)
            {
                var token_data = (JObject)JsonConvert.DeserializeObject(token_response.Content.ToString());
                crm_token = token_data["access_token"].Value<string>();
            }
            return crm_token;
        }
    }
}