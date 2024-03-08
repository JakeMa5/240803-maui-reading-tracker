
using Newtonsoft.Json.Linq;

namespace ReadingTracker.Requests
{
    class RequestHandler
    {
        public static async Task<string> SearchBooks(string searchQuery)
        {
            string apiKey = null;

            using (var httpClient = new HttpClient())
            {
                try
                {
                    string url = $"https://openlibrary.org/search.json?q={searchQuery}&limit=32";

                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        // If an API key is required, add it to the URL
                        url += $"&api_key={apiKey}";
                    }

                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Throw on error

                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                    return null;
                }
            }
        }

        public static async Task<List<string>> GetBookNames(string searchTerm)
        {
            List<string> bookNames = new List<string>();

            using (HttpClient client = new HttpClient())
            {
                string url = $"https://openlibrary.org/search.json?q={searchTerm}&limit=3";

                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseBody);

                    JArray docs = (JArray)json["docs"];
                    foreach (JToken doc in docs)
                    {
                        string title = GetStringFromToken(doc, "title");
                        if (!string.IsNullOrEmpty(title))
                        {
                            bookNames.Add(title);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }

            return bookNames;

        }

        static string GetStringFromToken(JToken token, string propertyName)
        {
            if (token[propertyName] != null)
            {
                return token[propertyName].ToString();
            }
            return null;
        }

    }
}
