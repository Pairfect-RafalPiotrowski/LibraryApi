using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LibraryApiClient.Models;

namespace LibraryApiClient
{
    public class Client
    {
        private readonly HttpClient _httpClient;

        public Client(string baseUrl, string bearerToken)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        protected Client(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            var response = await _httpClient.GetAsync("/api/books");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Book>>(content) ?? new List<Book>();
        }

        public async Task<List<Order>> GetAllOrdersAsync(int pageSize)
        {
            var allOrders = new List<Order>();
            int pageNumber = 1;

            while (true)
            {
                var response = await _httpClient.GetAsync($"/api/orders?pageSize={pageSize}&pageNumber={pageNumber}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<Order>>(content) ?? new List<Order>();

                if (orders.Count == 0)
                {
                    break;
                }

                allOrders.AddRange(orders);
                pageNumber++;
            }

            return allOrders;
        }

        public async Task AddBookAsync(Book book)
        {
            var content = new StringContent(JsonSerializer.Serialize(book), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/books", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
