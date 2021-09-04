using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Moq;
using Moq.Protected;
using WordFinder5000.Core;
using Xunit;

namespace WordFinder5000.Tests
{
    public class BookRepoTests
    {
        private readonly Faker _faker;
        private readonly string _testData;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly BookRepo _bookRepo;
        private readonly string _url;

        public BookRepoTests()
        {
            _faker = new Faker();
            
            _testData = _faker.Random.String2(10);
            _url = _faker.Internet.Url();
            
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(s => s.RequestUri == new Uri(_url) && s.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(_testData)
                });

            _bookRepo = new BookRepo(new HttpClient(_httpMessageHandlerMock.Object));
        }

        [Fact]
        public async Task GetBook_OkResult_ShouldReturnExpectedTestData()
        {
            var result = await _bookRepo.GetBookAsync(_url);

            result.Should().BeEquivalentTo(_testData);
        }

        [Fact]
        public async Task GetBook_InvalidUrl_ShouldThrowInvalidOperationException()
        {
            Func<Task> func = async () =>  await _bookRepo.GetBookAsync(_faker.Random.String2(10));

            await func.Should().ThrowAsync<InvalidOperationException>();
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task Get_Book_NonOkResult_ShouldThrowExceptionWhenHttpCallIsUnsuccessful(HttpStatusCode statusCode)
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(_testData)
                });
            Func<Task> fun = async () => await _bookRepo.GetBookAsync(_url);

            await fun.Should().ThrowAsync<Exception>();
        }
    }
}