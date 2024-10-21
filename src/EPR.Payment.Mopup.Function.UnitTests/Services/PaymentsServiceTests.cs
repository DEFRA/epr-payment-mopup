using AutoFixture.MSTest;
using AutoMapper;
using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Common.Data.Interfaces.Repositories;
using EPR.Payment.Mopup.Common.Data.Profiles;
using EPR.Payment.Mopup.Common.Dtos.Request;
using EPR.Payment.Mopup.Common.Dtos.Response;
using EPR.Payment.Mopup.Common.Enums;
using EPR.Payment.Mopup.Common.Mappers;
using EPR.Payment.Mopup.Common.RESTServices.Interfaces;
using EPR.Payment.Mopup.Common.UnitTests.TestHelpers;
using EPR.Payment.Mopup.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Mopup.UnitTests.Services
{
    [TestClass]
    public class PaymentsServiceTests
    {
        private IMapper _mapper = null!;
        private Mock<IPaymentsRepository> _paymentRepositoryMock = null!;
        private Mock<IHttpGovPayService> _httpGovPayServiceMock = null!;
        private Mock<ILogger<PaymentsService>> _loggerMock = null!;
        private PaymentsService _paymentsService = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _paymentRepositoryMock = new Mock<IPaymentsRepository>();
            _httpGovPayServiceMock = new Mock<IHttpGovPayService>();
            _loggerMock = new Mock<ILogger<PaymentsService>>();

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
                );

        }

        [TestMethod]
        public void Constructor_WhenAllDependenciesAreNotNull_ShouldCreateInstance()
        {
            // Act
            var service = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
                );

            // Assert
            service.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WhenHttpGovPayServiceIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new PaymentsService(
                _paymentRepositoryMock.Object,
                null!,
                _loggerMock.Object,
                _mapper
                );

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpGovPayService");
        }


        [TestMethod]
        public void Constructor_WhenPaymentRepositoryIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new PaymentsService(
                 null!,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
                );

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("paymentRepository");
        }

        [TestMethod]
        public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                null!,
                _mapper
                );

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [TestMethod]
        public void Constructor_WhenMapperIsNull_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                 null!
                );

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldUpdatePaymentStatus(
            [Frozen] List<Common.Data.DataModels.OnlinePayment> _payments,
            [Frozen] PaymentStatusResponseDto _paymentStatusResponseDto)
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(x => x.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_payments);

            List<PaymentDto> _paymentDtos = _mapper.Map<List<PaymentDto>>(_payments);

            foreach (var paymentDto in _paymentDtos)
            {
                _paymentStatusResponseDto!.State!.Status = "success";
                _paymentStatusResponseDto!.State!.Code = "";
                _paymentStatusResponseDto!.State!.Message = "Payment Successful";

                _httpGovPayServiceMock
                    .Setup(x => x.GetPaymentStatusAsync(paymentDto.GovpayPaymentId!, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_paymentStatusResponseDto);

                paymentDto.Status = PaymentStatusMapper.GetPaymentStatus(_paymentStatusResponseDto!.State);

                UpdatePaymentRequestDto _updatePaymentRequestDto = _mapper.Map<UpdatePaymentRequestDto>(paymentDto);

                var entity = _payments.SingleOrDefault(x => x.ExternalPaymentId == paymentDto.ExternalPaymentId);

                _mapper.Map(_updatePaymentRequestDto, entity);
            }

            // Act
            await _paymentsService.UpdatePaymentsAsync();

            // Assert
            using (new AssertionScope())
            {
                _paymentRepositoryMock.Verify(x => x.UpdatePaymentStatusAsync(It.IsAny<Common.Data.DataModels.OnlinePayment>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_WhenGovPayPaymentIdIsNull_ShouldLogError()
        {
            // Arrange
            var payments = new List<Common.Data.DataModels.OnlinePayment>
            {
                new Common.Data.DataModels.OnlinePayment{GovpayPaymentId = null}
            };

            _paymentRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payments);

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
            );

            // Act
            await _paymentsService.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            _loggerMock.Verify(
                 x => x.Log(
                     It.Is<LogLevel>(l => l == LogLevel.Error),
                     It.IsAny<EventId>(),
                     It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.PaymentIdNotFound)),
                     It.IsAny<Exception?>(),
                     It.Is<Func<It.IsAnyType, Exception?, string>>((state, exception) => true)
                 ),
                 Times.Once()
             );
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_WhenGovPayPaymentIdIsEmptyString_ShouldLogError()
        {
            // Arrange
            var payments = new List<Common.Data.DataModels.OnlinePayment>
            {
                new Common.Data.DataModels.OnlinePayment{GovpayPaymentId = string.Empty}
            };

            _paymentRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payments);

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
            );

            // Act
            await _paymentsService.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            _loggerMock.Verify(
                 x => x.Log(
                     It.Is<LogLevel>(l => l == LogLevel.Error),
                     It.IsAny<EventId>(),
                     It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.PaymentIdNotFound)),
                     It.IsAny<Exception?>(),
                     It.Is<Func<It.IsAnyType, Exception?, string>>((state, exception) => true)
                 ),
                 Times.Once()
             );
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_WhenPaymentsListIsEmpty_ShouldNotCallOtherMethods(
            [Frozen] Mock<IMapper> _mapperMock)
        {
            // Arrange
            _paymentRepositoryMock.Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(new List<Common.Data.DataModels.OnlinePayment>());

            // Act
            await _paymentsService.UpdatePaymentsAsync();

            // Assert
            using(new AssertionScope())
            {
                _httpGovPayServiceMock.Verify(service => service.GetPaymentStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
                _mapperMock.Verify(mapper => mapper.Map<UpdatePaymentRequestDto>(It.IsAny<PaymentDto>()), Times.Never);
            }

        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_WhenGovpayPaymentIdIsEmpty_ShouldLogErrorAndContinue(
            [Frozen] List<Common.Data.DataModels.OnlinePayment> _payments,
            [Frozen] List<PaymentDto> _paymentDtos,
            [Frozen] Mock<IMapper> _mapperMock)
        {
            // Arrange
            _paymentDtos[0].GovpayPaymentId = null;
            _payments[0].GovpayPaymentId = null;

            _paymentRepositoryMock.Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(_payments);
            _mapperMock.Setup(mapper => mapper.Map<List<PaymentDto>>(It.IsAny<IEnumerable<Common.Data.DataModels.OnlinePayment>>()))
                       .Returns(_paymentDtos);

            // Act
            await _paymentsService.UpdatePaymentsAsync();

            // Assert
            using (new AssertionScope())
            {
                _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ExceptionMessages.PaymentIdNotFound)),
                    null,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
                _httpGovPayServiceMock.Verify(service => service.GetPaymentStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            }
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_WhenGetPaymentStatusResponseAsyncThrowsException_ShouldLogErrorAndContinue(
            [Frozen] List<Common.Data.DataModels.OnlinePayment> _payments,
            [Frozen] List<PaymentDto> _paymentDtos,
            [Frozen] Mock<IMapper> _mapperMock)
        {
            // Arrange

            _paymentRepositoryMock.Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(_payments);
            _mapperMock.Setup(mapper => mapper.Map<List<PaymentDto>>(It.IsAny<IEnumerable<Common.Data.DataModels.OnlinePayment>>()))
                       .Returns(_paymentDtos);

            _httpGovPayServiceMock.Setup(service => service.GetPaymentStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _paymentsService.UpdatePaymentsAsync();

            // Assert
            using(new AssertionScope())
            {
                _loggerMock.Verify(
              logger => logger.Log(
                  LogLevel.Error,
                  It.IsAny<EventId>(),
                  It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while retrieving payment status.")),
                  It.IsAny<Exception>(),
                  It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
              Times.Exactly(6));

                _httpGovPayServiceMock.Verify(service => service.GetPaymentStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            }
           
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_WhenPaymentIsProcessedSuccessfully_ShouldUpdatePaymentStatusAndLogInformation(
            [Frozen] List<Common.Data.DataModels.OnlinePayment> _payments,
            [Frozen] List<PaymentDto> _paymentDtos,
            [Frozen] Mock<IMapper> _mapperMock,
            [Frozen] PaymentStatusResponseDto _paymentStatusResponse)
        {
            // Arrange
            _paymentStatusResponse.State!.Status = "Success";
            _paymentStatusResponse.State!.Code = string.Empty;
            _paymentRepositoryMock.Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(_payments);
            _mapperMock.Setup(mapper => mapper.Map<List<PaymentDto>>(It.IsAny<IEnumerable<Common.Data.DataModels.OnlinePayment>>()))
                       .Returns(_paymentDtos);

            _httpGovPayServiceMock.Setup(service => service.GetPaymentStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(_paymentStatusResponse);

            // Act
            await _paymentsService.UpdatePaymentsAsync();

            // Assert
            using(new AssertionScope())
            {
                _paymentRepositoryMock.Verify(repo => repo.UpdatePaymentStatusAsync(It.IsAny<Common.Data.DataModels.OnlinePayment>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

                _loggerMock.Verify(
                    logger => logger.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("externalPaymentId has been updated")),
                        null,
                        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                    Times.Exactly(3));
            }
        }

    }
}
