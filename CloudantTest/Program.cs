using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CloudantTest
{
    class Program
    {
        static void Main(string[] args)
        {

            var user = "55bd2041-ef4a-4dec-8bc1-b4aa8871b38f-bluemix";
            var password = "cbcdf64595e4ed9d5546e7f9761a03153c3246d97458734c58cfef0f7b06da63";
            var database = "database";
            var handler = new HttpClientHandler { Credentials = new NetworkCredential(user, password) };

            using (var client = CreateHttpClient(handler, user, database))
            {

                string GetAllResponse = GetAll(client);//Query for all docs
                Console.Write(GetAllResponse);
                var creationResponse = Create(client, new {item = "carrot", check = false});
                PrintResponse(creationResponse);

                var id = GetString("id", creationResponse);//Read a doc
                var readResponse = Read(client, id);
                PrintResponse(readResponse);

               var rev1 = GetString("_rev", readResponse);//Update
                var updateResponse = Update(client, id, new {item = "carrot", check = true, _rev = rev1});
                PrintResponse(updateResponse);
                
                var rev2 = GetString("rev", updateResponse); //Delete
                var deleteResponse = Delete(client, id, rev2);
                PrintResponse(deleteResponse);
                Console.Read();
            }
        }


        private static string GetAll(HttpClient client)
        {
           
            using (var streamReader = new StreamReader(client.GetStreamAsync("https://55bd2041-ef4a-4dec-8bc1-b4aa8871b38f-bluemix.cloudant.com/database/_all_docs").Result))
            {
                var responseContent = (JObject)JToken.ReadFrom(new JsonTextReader(streamReader));
                return responseContent.ToString();
            }
            

        }
        private static HttpResponseMessage Create(HttpClient client, object doc)
        {
            var json = JsonConvert.SerializeObject(doc);
            return client.PostAsync("", new StringContent(json, Encoding.UTF8, "application/json")).Result;
        }

        private static HttpResponseMessage Read(HttpClient client, string id)
        {
            return client.GetAsync(id).Result;
        }

        private static HttpResponseMessage Update(HttpClient client, string id, object doc)
        {
            var json = JsonConvert.SerializeObject(doc);
            return client.PutAsync(id, new StringContent(json, Encoding.UTF8, "application/json")).Result;
        }

        private static HttpResponseMessage Delete(HttpClient client, string id, string rev)
        {
            return client.DeleteAsync(id + "?rev=" + rev).Result;
        }

        private static HttpClient CreateHttpClient(HttpClientHandler handler, string user, string database)
        {
            return new HttpClient(handler)
            {
                BaseAddress = new Uri(string.Format("https://{0}.cloudant.com/{1}/", user, database))
            };
        }

        private static void PrintResponse(HttpResponseMessage response)
        {
            Console.WriteLine("Status code: {0}", response.StatusCode);
            Console.WriteLine(Convert.ToString(response));
        }
        
        private static string GetString(string propertyName, HttpResponseMessage creationResponse)
        {
            using (var streamReader = new StreamReader(creationResponse.Content.ReadAsStreamAsync().Result))
            {
                var responseContent = (JObject) JToken.ReadFrom(new JsonTextReader(streamReader));
                return responseContent[propertyName].Value<string>();
            }
        }
    }
}
