using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;

namespace LibraryTests
{
    public static class HttpMockHandlerHelper
    {
        public static Mock<HttpMessageHandler> SetupMockHttpHandlerForGet(HttpResponseMessage responseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            return handlerMock;
        }

        public static Mock<HttpMessageHandler> SetupMockHttpHandlerForPost<T>(T content, HttpStatusCode statusCode)
        {
            var serializedContent = JsonSerializer.Serialize(content);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.Content != null &&
                        req.Content.ReadAsStringAsync().Result == serializedContent),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode
                });

            return handlerMock;
        }

        public static Mock<HttpMessageHandler> SetupMockHttpHandlerForPagination<T>(List<T> pages, HttpStatusCode statusCode)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var sequence = handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());

            foreach (var page in pages)
            {
                sequence = sequence.ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(JsonSerializer.Serialize(page))
                });
            }

            sequence.ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent("[]")
            });

            return handlerMock;
        }
    }
}