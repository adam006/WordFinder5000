using System.Net.Http;
using System.Threading.Tasks;

namespace WordFinder5000.Core
{
    public interface IBookRepo
    {
        Task<string> GetBookAsync(string url);
    }

    public class BookRepo : IBookRepo
    {
        private readonly HttpClient _client;

        public BookRepo(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> GetBookAsync(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return body;
        }
    }
}