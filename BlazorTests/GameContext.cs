using System.Net.Http;

namespace BlazorTests {
    public class GameContext {
        private readonly HttpClient _client;
        
        public GameContext(HttpClient client) {
            _client = client;
        }

        public void Test() {
        }
    }
}
