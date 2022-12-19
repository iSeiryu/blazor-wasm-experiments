using System.Net.Http;

namespace BlazorExperiments.UI;

public class GameContext
{
    private readonly HttpClient _client;

    public GameContext(HttpClient client)
    {
        _client = client;
    }

    public void Test()
    {
    }
}
