using System.Net;
using System.Text.Json;
using LibraryApiClient;
using LibraryApiClient.Models;
using Moq;
using Moq.Protected;

namespace LibraryTests
{
    public class ClientTests
    {
        private static HttpClient CreateMockHttpClient(Mock<HttpMessageHandler> handlerMock)
        {
            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };
        }

        [Fact]
        public async Task GetBooksAsync_ShouldReturnListOfBooks()
        {
            var mockBooks = new List<Book>
            {
                new Book { Id = 1, Title = "Book 1", Price = 5 },
                new Book { Id = 2, Title = "Book 2", Price = 6 }
            };
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(mockBooks))
            };

            var handlerMock = HttpMockHandlerHelper.SetupMockHttpHandlerForGet(responseMessage);
            var httpClient = CreateMockHttpClient(handlerMock);
            var client = new TestableClient(httpClient);

            var books = await client.GetBooksAsync();

            Assert.Equal(2, books.Count);
            Assert.Equal("Book 1", books[0].Title);
            Assert.Equal("Book 2", books[1].Title);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
        {
            var mockOrdersPage1 = new List<Order>
            {
                new Order
                {
                    OrderId = Guid.NewGuid(),
                    OrderLines = new List<OrderLine>
                    {
                        new OrderLine { BookId = 1, Quantity = 2 }
                    }
                }
            };
            var mockOrdersPage2 = new List<Order>
            {
                new Order
                {
                    OrderId = Guid.NewGuid(),
                    OrderLines = new List<OrderLine>
                    {
                        new OrderLine { BookId = 2, Quantity = 3 }
                    }
                }
            };

            var handlerMock = HttpMockHandlerHelper.SetupMockHttpHandlerForPagination(new List<List<Order>> { mockOrdersPage1, mockOrdersPage2 }, HttpStatusCode.OK);
            var httpClient = CreateMockHttpClient(handlerMock);
            var client = new TestableClient(httpClient);

            // Act
            var orders = await client.GetAllOrdersAsync(1);

            // Assert
            Assert.Equal(2, orders.Count);
            Assert.Equal(1, orders[0].OrderLines[0].BookId);
            Assert.Equal(3, orders[1].OrderLines[0].Quantity);
        }

        [Fact]
        public async Task AddBookAsync_ShouldSendPostRequest()
        {
            // Arrange
            var mockBook = new Book
            {
                Id = 1,
                Title = "New Book",
                Price = 20
            };

            var handlerMock = HttpMockHandlerHelper.SetupMockHttpHandlerForPost(mockBook, HttpStatusCode.Created);
            var httpClient = CreateMockHttpClient(handlerMock);
            var client = new TestableClient(httpClient);

            // Act
            await client.AddBookAsync(mockBook);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("http://localhost/api/books")),
                ItExpr.IsAny<CancellationToken>());
        }
    }

    internal class TestableClient : Client
    {
        public TestableClient(HttpClient httpClient) : base(httpClient) { }
    }
}
