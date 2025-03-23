using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitHubUserActivity
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // https://api.github.com/users/tater/events
            Console.WriteLine("Enter github username: ");
            string? username = Console.ReadLine();
            string url = $"https://api.github.com/users/{username}/events";
            try
            {
                GetAPIResponse(url).Wait();
            }
            catch (Exception ex)
            {
                //if (ex.Message.Contains("Not Found"))
                //{
                //    Console.WriteLine("User not found");
                //    Console.WriteLine("Please enter correct username: ");
                //    username = Console.ReadLine();
                //    url = $"https://api.github.com/users/{username}/events";
                //    GetAPIResponse(url).Wait();
                //}
                //else
                //{
                //    Console.WriteLine("Something went wrong. Try again.");
                //}
                Environment.Exit(0);
            }
        }

        static async Task GetAPIResponse(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    List<JObject>? events = JsonConvert.DeserializeObject<List<JObject>>(jsonResponse);

                    if (events == null)
                    {
                        Console.WriteLine("No data found");
                        return;
                    }

                    Dictionary<string, int> repoCommits = new Dictionary<string, int>();

                    foreach (var obj in events)
                    {
                        string? type = obj["type"].ToString();
                        var repoName = obj["repo"]["name"].ToString();

                        if (string.IsNullOrEmpty(type)) continue;

                        switch (type)
                        {
                            case "PushEvent":
                                if (!repoCommits.ContainsKey(repoName))
                                {
                                    repoCommits.Add(repoName, 1);
                                }
                                else
                                {
                                    repoCommits[repoName]++;
                                }
                                break;
                            case "WatchEvent":
                                Console.WriteLine($"Starred: {repoName}");
                                Console.WriteLine();
                                break;
                        }
                    }

                    if (repoCommits.Count != 0)
                    {
                        Console.WriteLine($"Made commits in:");
                        foreach (var repo in repoCommits)
                        {
                            Console.WriteLine($"Repo: {repo.Key}, Commits: {repo.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("User haven't made any commits in the last 90 days.");
                    }
                }
                catch (HttpRequestException e)
                {
                    if (e.Message.Contains("404"))
                    {
                        Console.WriteLine("User doesn't exist.");
                        Console.WriteLine("Try again later.");
                        throw new Exception("Error: " + e.Message);
                    }
                    else
                    {
                        throw new Exception("Error: " + e.Message);
                    }
                }
            }
        }
    }
}
