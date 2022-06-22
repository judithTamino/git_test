using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace app
{
    public class Github
    {
        private static string base_url = "https://api.github.com";
        private static string path_parameters = @"judithTamino/git_test";

        public async Task<Dictionary<string, string>> CreateBlob(Dictionary<string, string> convert_files)
        {
            string endpoint = $"{base_url}/repos/{path_parameters}/git/blobs";
            Dictionary<string, string> sha_list = new Dictionary<string, string>();

            foreach (var file in convert_files)
            {
                RestClient client = new RestClient(endpoint); 

                RestRequest request = new RestRequest();
                request.Method = Method.Post;
                request.AddHeader("Authorization", "Bearer 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");
                request.AddHeader("Accept", "application/vnd.github.v3+json");

                string data = Newtonsoft.Json.JsonConvert.SerializeObject(new {content = file.Value});
                request.AddParameter("application/json", data, ParameterType.RequestBody);

                RestResponse response = await client.ExecuteAsync(request);
                JToken content = JObject.Parse(response.Content);

                sha_list.Add(file.Key, content["sha"].ToString());
            }

            return sha_list;
        }

        public async Task<string> GetCurrentCommit()
        {
            string endpoint = "/repos/" + path_parameters + "/git/ref/heads/main";

            RestClient client = new RestClient(base_url + endpoint);

            RestRequest request = new RestRequest();
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddHeader("Authorization", "Bearer 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");

            RestResponse response = await client.GetAsync(request);

            JToken body = JObject.Parse(response.Content);
            string commit_url = body["object"]["url"].ToString();

            return commit_url;
        }

        public async Task<string> GetCurrentCommitTree(string commit_url)
        {
            RestClient client = new RestClient(commit_url);

            RestRequest request = new RestRequest();
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddHeader("Authorization", "Bearer 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");

            RestResponse response = await client.GetAsync(request);
            string base_tree = JObject.Parse(response.Content)["tree"]["sha"].ToString();

            return base_tree;
        }

        public async Task CreateTree(string base_tree_sha, Dictionary<string, string> blobs_sha_list)
        {
            string endpoin = $"{base_url}/repos/{path_parameters}/git/trees";

            int i = 0;
            string[] blobs_obj = new string[blobs_sha_list.Count];

            

            foreach (var sha in blobs_sha_list)
            {
                string data = Newtonsoft.Json.JsonConvert.SerializeObject(new { 
                    path = $"demo/demo/{sha.Key}", 
                    mode = "100644",
                    type = "blob",
                    sha = sha.Value,
                });

                blobs_obj[i] = data;
                i++;                
            }

            RestClient client = new RestClient(endpoin);

            RestRequest request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddHeader("Authorization", "Bearer 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");

            var body = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                tree = blobs_obj,
                base_tree = base_tree_sha
            });

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
        }




        /* public void SplitUrl(string file_url)
         {
             int index = file_url.IndexOf(repo);
             string str = file_url.Substring(index);
         }

         public async Task GetCurrentCommit()
         {
             string endpoint = "/repos/" + path_parameters + "/git/ref/heads/main";

             RestClient client = new RestClient(base_url + endpoint);

             RestRequest request = new RestRequest();
             request.AddHeader("Accept", "application/vnd.github.v3+json");
             request.AddHeader("Authorization", "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");

             RestResponse response = await client.GetAsync(request);

             JToken body = JObject.Parse(response.Content);
             string commit_url = body["object"]["url"].ToString();

             await GetCurrentCommitTree(commit_url);
         }

         public async Task GetCurrentCommitTree(string commit_url)
         {
             RestClient client = new RestClient(commit_url);

             RestRequest request = new RestRequest();
             request.AddHeader("Accept", "application/vnd.github.v3+json");
             request.AddHeader("Authorization", "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");

             RestResponse response = await client.GetAsync(request);
             JToken body = JObject.Parse(response.Content);

         }

         public void Octokit()
         {
             //GitHubClient client = new GitHubClient(new ProductHeaderValue(base_url));
         }
        */


        //private static string ENDPOINT = $"{base_url}/{owner}/conte" 

        /*public static string file;

        public async Task GetReferenceToHead()
        {
            string endpoint = "/repos/" + owner + "/" + repo + "/git/ref/heads/main";

            RestClient client = new RestClient(base_url);

            RestRequest request = new RestRequest(endpoint);
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddHeader("Authorization", "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");

            RestResponse response = await client.GetAsync(request);

            JToken body = JObject.Parse(response.Content);
            string commit_sha = body["object"]["sha"].ToString();
            string commit_url = body["object"]["url"].ToString();

            await GrabTheCommitThatHeadPointsTo(commit_url);
        }

        public async Task GrabTheCommitThatHeadPointsTo(string commit_url)
        {
            RestClient client = new RestClient(commit_url);

            RestRequest request = new RestRequest();
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddHeader("Authorization", "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");

            RestResponse response = await client.GetAsync(request);
            JToken body = JObject.Parse(response.Content);

            string commit_sha = body["sha"].ToString();
            string tree_sha = body["tree"]["sha"].ToString();
            string tree_url = body["tree"]["url"].ToString();

            await PostNewFile();
        }

        public async Task PostNewFile()
        {
            string endpoint = "/repos/" + owner + "/" + repo + "/git/blobs";
            string file_text = System.IO.File.ReadAllText(file);
            string new_blob = @"{
                                    'content':{file_text},
                                    'encoding':'utf-8'

                                }";

            RestClient client = new RestClient(base_url + endpoint);

            RestRequest request = new RestRequest();
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddHeader("Authorization", "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");
            request.AddParameter("application/json", new_blob,ParameterType.RequestBody);

            RestResponse response = await client.PostAsync(request);
        }
        

      

        public async Task GetTheCurrentCommitObj(string ref_sha)
        {
            RestClient client = new RestClient(base_url);

            RestRequest request = new RestRequest($"/repos/{owner}/{repo}/git/commits/{ref_sha}");
            //request.AddUrlSegment("sha", ref_sha);
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddHeader("Authorization", "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");

            RestResponse response = await client.GetAsync(request);
            var results = JObject.Parse(response.Content)["tree"]["sha"];

            string base_tree = results.ToString();
        }
        */
    }
}
