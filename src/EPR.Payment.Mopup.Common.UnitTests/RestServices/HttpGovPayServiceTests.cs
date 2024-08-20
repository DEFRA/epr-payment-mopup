using AutoFixture.MSTest;
using EPR.Payment.Mopup.Common.Configuration;
using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Common.Dtos.Response;
using EPR.Payment.Mopup.Common.RESTServices;
using EPR.Payment.Mopup.Common.UnitTests.TestHelpers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Payment.Mopup.Common.UnitTests.RestServices
{
    [TestClass]
    public class HttpGovPayServiceTests
    {
        private Mock<IHttpContextAccessor>? _httpContextAccessorMock;
        private Mock<IOptions<Service>>? _configMock;
        private PaymentStatusResponseDto? _expectedResponse;


        [TestInitialize]
        public void Initialize()
        {
            // Mock configuration
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "payments",
                BearerToken = "dummyBearerToken"
            };

            _configMock = new Mock<IOptions<Service>>();
            _configMock.Setup(x => x.Value).Returns(config);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _expectedResponse = new PaymentStatusResponseDto
            { 
                PaymentId = "12345",
                State = new State { Status = "created", Finished = false }
            };
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithValidParameters_ShouldNotThrowException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var serviceOptions = new Service
            {
                Url = "https://valid-url.com",
                EndPointName = "ValidEndPoint",
                BearerToken = "ValidToken"
            };

            _configMock!.Setup(x => x.Value).Returns(serviceOptions);
            // Act
            Action act = () => new HttpGovPayService(_httpContextAccessorMock!.Object, new HttpClientFactoryMock(new HttpClient(handlerMock.Object)), _configMock.Object);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullHttpContextAccessor_ShouldThrowArgumentNullException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var serviceOptions = new Service
            {
                Url = "https://valid-url.com",
                EndPointName = "ValidEndPoint",
                BearerToken = "ValidToken"
            };
            _configMock!.Setup(o => o.Value).Returns(serviceOptions);

            // Act
            Action act = () => new HttpGovPayService(null!, new HttpClientFactoryMock(new HttpClient(handlerMock.Object)), _configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("*httpContextAccessor*");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenBearerTokenIsNull_ThrowsArgumentNullException(
                            [Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var config = new Service
            {
                Url = "https://example.com",
                EndPointName = "payments",
                BearerToken = null // Simulate null BearerToken
            };
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(x => x.Value).Returns(config);

            // Act
            Action act = () => new HttpGovPayService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(new HttpClient(handlerMock.Object)),
                configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithMessage("*Bearer token is null. Unable to initiate payment.*")
               .WithParameterName("config");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullUrl_ShouldThrowArgumentNullException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var config = new Service
            {
                Url = null, // Url is null
                EndPointName = "ValidEndPoint",
                BearerToken = "ValidToken"
            };
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(x => x.Value).Returns(config);

            // Act
            Action act = () => new HttpGovPayService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(new HttpClient(handlerMock.Object)),
                configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("config");
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WithNullEndPointName_ShouldThrowArgumentNullException([Frozen] Mock<HttpMessageHandler> handlerMock)
        {
            // Arrange
            var config = new Service
            {
                Url = "https://valid-url.com",
                EndPointName = null, // EndPointName is null
                BearerToken = "ValidToken"
            };
            var configMock = new Mock<IOptions<Service>>();
            configMock.Setup(x => x.Value).Returns(config);

            // Act
            Action act = () => new HttpGovPayService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(new HttpClient(handlerMock.Object)),
                configMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("config");
        }

        private HttpGovPayService CreateHttpGovPayService(HttpClient httpClient)
        {
            return new HttpGovPayService(
                _httpContextAccessorMock!.Object,
                new HttpClientFactoryMock(httpClient),
                _configMock!.Object);
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatus_Success_ReturnsPaymentStatusResponseDto(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            string paymentId,
            HttpGovPayService httpGovPayService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                       ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(new HttpResponseMessage
                       {
                           StatusCode = HttpStatusCode.OK,
                           Content = new StringContent(JsonConvert.SerializeObject(_expectedResponse)),
                       }).Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            httpGovPayService = CreateHttpGovPayService(httpClient);

            // Act
            var result = await httpGovPayService.GetPaymentStatusAsync(paymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();

                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());

            };
        }

        [TestMethod, AutoMoqData]
        public async Task GetPaymentStatus_Failure_ThrowsException(
            [Frozen] Mock<HttpMessageHandler> handlerMock,
            string paymentId,
            HttpGovPayService httpGovPayService,
            CancellationToken cancellationToken)
        {
            // Arrange
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ThrowsAsync(new HttpRequestException(ExceptionMessages.ErrorRetrievingPaymentStatus));

            var httpClient = new HttpClient(handlerMock.Object);
            httpGovPayService = CreateHttpGovPayService(httpClient);

            // Act
            Func<Task> act = async () => await httpGovPayService.GetPaymentStatusAsync(paymentId, cancellationToken);

            // Assert
            using (new AssertionScope())
            {
                await act.Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);
                handlerMock.Protected().Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>());
            }
        }
    }
}
