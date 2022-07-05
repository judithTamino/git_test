using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace app
{
    public class APIHelper
    {
        private string base_url = "https://api.github.com/repos/";
        private string path_parameters = @"judithTamino/Demo_Project/";

        public RestClient SetUrl(string endpoint)
        {
            string url = base_url + path_parameters + endpoint;
            RestClient client = new RestClient(url);

            return client;
        }

        public RestRequest CreatePostRequest(string payload)
        {
            RestRequest request = new RestRequest();

            request.Method = Method.Post;
            request.AddHeader("Authorization", "Bearer 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");
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddParameter("application/json", payload, ParameterType.RequestBody);

            return request;
        }

        public RestRequest CreateGetRequest()
        {
            RestRequest request = new RestRequest();

            request.Method = Method.Get;
            request.AddHeader("Authorization", "Bearer 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");
            request.AddHeader("Accept", "application/vnd.github.v3+json");

            return request;
        }

        public RestRequest CreatePatchRequest(string payload)
        {
            RestRequest request = new RestRequest();

            request.Method = Method.Patch;
            request.AddHeader("Authorization", "Bearer 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");
            request.AddHeader("Accept", "application/vnd.github.v3+json");
            request.AddParameter("application/json", payload, ParameterType.RequestBody);

            return request;
        }

        public async Task <RestResponse> GetResponse(RestClient client, RestRequest request)
        {
            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public JToken GetContent (RestResponse response)
        {
            var content = response.Content;
            if (content[0].Equals('['))
                return JArray.Parse(content);
            return JObject.Parse(content);
        }
    }
}
