using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace app
{
    public class Github
    {
        private APIHelper api = new APIHelper();
        private string _commitSHA;
        public async Task<Dictionary<string, string>> CreateBlob(Dictionary<string, string> encodedFiles)
        {
            string endpoint = @"git/blobs";
            Dictionary<string, string> filesSha = new Dictionary<string, string>();

            foreach (var file in encodedFiles)
            {
                string payload = JsonConvert.SerializeObject(new { content = file.Value, encoding = "base64" });
                JToken content = await PostRequeset(endpoint, payload);

                if (content.HasValues)
                    filesSha.Add(file.Key, content["sha"].ToString());
            }
            return filesSha;
        }

        public async Task<string> GetCurrentCommit()
        {
            string commitSha = "";
            string endpoint = @"git/ref/heads/main";
            JToken content = await GetRequeset(endpoint);

            if (content.HasValues)
                commitSha = content["object"]["sha"].ToString();
            _commitSHA = commitSha;

            return commitSha;
        }

        public async Task<string> GetBaseTree(string currentCommitSha)
        {
            string endpoint = $"git/commits/{currentCommitSha}";
            JToken content = await GetRequeset(endpoint);

            if (content.HasValues)
                return content["tree"]["sha"].ToString();

            return "";
        }

        public async Task<string> CreateTree(string baseTreeSHA, Dictionary<string, string> blobsList)
        {
            string endpoint = @"git/trees";
            object[] blobsObjectList = new object[blobsList.Count];
            int i = 0;

            foreach (var sha in blobsList)
            {
                var data = new { path = sha.Key, mode = "100644", type = "blob", sha = sha.Value };

                blobsObjectList[i] = data;
                i++;
            }

            string payload = JsonConvert.SerializeObject(new { tree = blobsObjectList, base_tree = baseTreeSHA });
            JToken content = await PostRequeset(endpoint, payload);


            if (content.HasValues)
                return content["sha"].ToString();
            
            return "";
        }

        public async Task<string> AddCommit(string newTreeSHA, string currentCommitSha)
        {
            string endpoint = @"git/commits";
            Guid guid = Guid.NewGuid();

            string payload = JsonConvert.SerializeObject(new { tree = newTreeSHA, message = $"commit N.{guid}", parents = new string[] { currentCommitSha } });
            JToken content = await PostRequeset(endpoint, payload);


            if (content.HasValues)
                return content["sha"].ToString();

            return "";
        }

        public async void UpdateRef(string newCommitSHA)
        {
            string endpoint = @"git/refs/heads/main";
            string payload = JsonConvert.SerializeObject(new { sha = newCommitSHA });

            JToken content = await PostRequeset(endpoint, payload);

            if (content.HasValues)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("The files were uploaded successfully");
                Console.ResetColor();
            }
        }

     

        private async Task<JToken> PostRequeset(string endpoint, string payload)
        {
            JToken content = new JObject();

            RestClient client = api.SetUrl(endpoint);
            RestRequest request = api.CreatePostRequest(payload);
            RestResponse response = await api.GetResponse(client, request);

            if (response.StatusCode.Equals(HttpStatusCode.Created) || response.StatusCode.Equals(HttpStatusCode.OK))
                content = api.GetContent(response);

            return content;
        }

        private async Task<JToken> GetRequeset(string endpoint)
        {
            JToken content = new JObject();

            RestClient client = api.SetUrl(endpoint);
            RestRequest request = api.CreateGetRequest();
            RestResponse response = await api.GetResponse(client, request);

            if (response.StatusCode.Equals(HttpStatusCode.OK))
                content = api.GetContent(response);

            return content;
        }
    }
}
