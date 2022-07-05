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

        public async Task<bool> UpdateRef(string newCommitSHA)
        {
            string endpoint = @"git/refs/heads/main";
            string payload = JsonConvert.SerializeObject(new { sha = newCommitSHA });

            JToken content = await PostRequeset(endpoint, payload);

            if (content.HasValues)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("The files were uploaded successfully");
                Console.ResetColor();

                return true;
            }

            return false;
        }

        public async Task<string> CheckRepoChanges()
        {
            string firstCommit = "";
            string endpoint = @"commits";
            JToken content = await GetRequeset(endpoint);


            if (content.HasValues)
                firstCommit = content[0]["sha"].ToString();

            return firstCommit;
        } 

        public async Task<int> GetCommits()
        {
            int countCommits = 0;
            string endpoint = @"commits";
            JToken content = await GetRequeset(endpoint);

            foreach (var commit in content)
                countCommits++;

            return countCommits;
        }

        public async Task<string[]> CreateTagObject()
        {
            Guid guid = Guid.NewGuid();
            string[] tagData = new string[2];

            // get current commit
            string currentCommit = await GetCurrentCommit();
            string endpoint = @"git/tags";

            // create tag
            JObject container = new JObject();
            JProperty tag = new JProperty("tag", "V." + guid);
            JProperty message = new JProperty("message", "initial version");
            JProperty obj = new JProperty("object", currentCommit);
            JProperty type = new JProperty("type", "commit");

            container.Add(tag);
            container.Add(message);
            container.Add(obj);
            container.Add(type);

            string payload = container.ToString();
            JToken content = await PostRequeset(endpoint, payload);

            if (content.HasValues)
            {
                tagData[0] = content["sha"].ToString();
                tagData[1] = content["tag"].ToString();
            }
            return tagData;
        }

        public async void AppendTag(string[] tag)
        {
            string endpoint = @"git/refs";

            JObject payload = new JObject();

            JProperty reference = new JProperty("ref", $"refs/tags/{tag[1]}");
            JProperty sha = new JProperty("sha", tag[0]);

            payload.Add(reference);
            payload.Add(sha);

            JToken content = await PostRequeset(endpoint, payload.ToString());

            if (content.HasValues)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Tag created succssefully");
                Console.ResetColor();
            }
        }

        /*public async void CreatePullRequest()
        {
            string endpoint = @"pulls";
            string payload = @"{
                                    " + "\n" +
                                    @"   ""head"": ""main"", 
                                    " + "\n" +
                                    @"   ""base"": ""main"",
                                    " + "\n" +
                                    @"   ""title"": ""My Test Pull Request""
                                    " + "\n" +
                                    @"}";
            JToken content = await PostRequeset(endpoint, payload);
        }*/

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
