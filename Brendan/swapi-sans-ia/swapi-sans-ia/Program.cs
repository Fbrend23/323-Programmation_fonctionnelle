private HttpClient client = new HttpClient();

async Task<string> HttpGetAsync(HttpClient client, string query)
{
    var response = await client.GetAsync(query.Contains("https") ? query : "https://swapi.dev/api/" + query);
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();

    return json;
}