using AutoFixture.MSTest;
using AutoMapper;
using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Common.Data.Interfaces.Repositories;
using EPR.Payment.Mopup.Common.Data.Profiles;
using EPR.Payment.Mopup.Common.Dtos.Request;
using EPR.Payment.Mopup.Common.Dtos.Response;
using EPR.Payment.Mopup.Common.Enums;
using EPR.Payment.Mopup.Common.Exceptions;
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
            [Frozen] List<Common.Data.DataModels.Payment> _payments,
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
                _paymentRepositoryMock.Verify(x => x.UpdatePaymentStatusAsync(It.IsAny<Common.Data.DataModels.Payment>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenGovPayPaymentIdIsNull()
        {
            // Arrange
            var payments = new List<Common.Data.DataModels.Payment>
            {
                new Common.Data.DataModels.Payment{GovpayPaymentId = null}
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
            Func<Task> action = async () => await _paymentsService.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage(ExceptionMessages.PaymentIdNotFound);

        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenGovPayPaymentIdIsEmptyString()
        {
            // Arrange
            var payments = new List<Common.Data.DataModels.Payment>
            {
                new Common.Data.DataModels.Payment{GovpayPaymentId = string.Empty}
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
            Func<Task> action = async () => await _paymentsService.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage(ExceptionMessages.PaymentIdNotFound);

        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenPaymentStatusResponseIsNull(
            [Frozen] List<Common.Data.DataModels.Payment> _payments)
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_payments);

            var paymentDtos = _mapper.Map<List<PaymentDto>>(_payments);

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
            );

            foreach (var paymentDto in paymentDtos)
            {
                _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(paymentDto.GovpayPaymentId!, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((PaymentStatusResponseDto?)null);

            }

            // Act
            Func<Task> action = async () => await _paymentsService.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);

        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenPaymentStateIsNull(
              [Frozen] List<Common.Data.DataModels.Payment> _payments,
              [Frozen] PaymentStatusResponseDto _paymentStatusResponseDto)
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_payments);

            var paymentDtos = _mapper.Map<List<PaymentDto>>(_payments);

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
            );

            foreach (var paymentDto in paymentDtos)
            {
                _paymentStatusResponseDto.State = null;
                _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(paymentDto.GovpayPaymentId!, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_paymentStatusResponseDto);

            }

            // Act & Assert
            await _paymentsService.Invoking(async s => await s.UpdatePaymentsAsync(new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenPaymentStatusIsNull(
             [Frozen] List<Common.Data.DataModels.Payment> _payments,
             [Frozen] PaymentStatusResponseDto _paymentStatusResponseDto)
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_payments);

            var paymentDtos = _mapper.Map<List<PaymentDto>>(_payments);

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
            );

            foreach (var paymentDto in paymentDtos)
            {
                _paymentStatusResponseDto.State = new State { Status = null, Finished = true };

                _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(paymentDto.GovpayPaymentId!, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_paymentStatusResponseDto);

            }

            // Act
            Func<Task> action = async () => await _paymentsService.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenPaymentStatusInvalid(
             [Frozen] List<Common.Data.DataModels.Payment> _payments,
             [Frozen] PaymentStatusResponseDto _paymentStatusResponseDto)
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_payments);

            var paymentDtos = _mapper.Map<List<PaymentDto>>(_payments);

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
            );

            foreach (var paymentDto in paymentDtos)
            {
                _paymentStatusResponseDto.State = new State { Status = "invalid_status", Finished = true };

                _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(paymentDto.GovpayPaymentId!, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_paymentStatusResponseDto);

            }

            // Act & Assert
            await _paymentsService.Invoking(async s => await s.UpdatePaymentsAsync(new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenSuccessStatusWithErrorCode(
             [Frozen] List<Common.Data.DataModels.Payment> _payments,
             [Frozen] PaymentStatusResponseDto _paymentStatusResponseDto)
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_payments);

            var paymentDtos = _mapper.Map<List<PaymentDto>>(_payments);

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
            );

            foreach (var paymentDto in paymentDtos)
            {
                _paymentStatusResponseDto.State = new State { Status = "success", Code = "SomeErrorCode", Finished = true };

                _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(paymentDto.GovpayPaymentId!, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_paymentStatusResponseDto);

            }

            // Act & Assert
            await _paymentsService.Invoking(async s => await s.UpdatePaymentsAsync(new CancellationToken()))
                .Should().ThrowAsync<Exception>().WithMessage(ExceptionMessages.SuccessStatusWithErrorCode);
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenFailedStatusWithEmptyErrorCode(
             [Frozen] List<Common.Data.DataModels.Payment> _payments,
             [Frozen] PaymentStatusResponseDto _paymentStatusResponseDto)
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_payments);

            var paymentDtos = _mapper.Map<List<PaymentDto>>(_payments);

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
            );

            foreach (var paymentDto in paymentDtos)
            {
                _paymentStatusResponseDto.State = new State { Status = "failed", Finished = true, Code = null };

                _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(paymentDto.GovpayPaymentId!, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_paymentStatusResponseDto);

            }

            // Act
            Func<Task> action = async () => await _paymentsService.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage(ExceptionMessages.FailedStatusWithoutErrorCode);
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenErrorStatusWithEmptyErrorCode(
             [Frozen] List<Common.Data.DataModels.Payment> _payments,
             [Frozen] PaymentStatusResponseDto _paymentStatusResponseDto)
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_payments);

            var paymentDtos = _mapper.Map<List<PaymentDto>>(_payments);

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
            );

            foreach (var paymentDto in paymentDtos)
            {
                _paymentStatusResponseDto.State = new State { Status = "error", Finished = true, Code = null };

                _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(paymentDto.GovpayPaymentId!, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_paymentStatusResponseDto);

            }

            // Act
            Func<Task> action = async () => await _paymentsService.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage(ExceptionMessages.ErrorStatusWithoutErrorCode);
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenUnknownStatus(
            [Frozen] List<Common.Data.DataModels.Payment> _payments,
            [Frozen] PaymentStatusResponseDto _paymentStatusResponseDto)
        {
            // Arrange
            _paymentRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_payments);

            var paymentDtos = _mapper.Map<List<PaymentDto>>(_payments);

            _paymentsService = new PaymentsService(
                _paymentRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
            );

            foreach (var paymentDto in paymentDtos)
            {
                _paymentStatusResponseDto.State = new State { Status = "unknown_status", Finished = true };

                _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(paymentDto.GovpayPaymentId!, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(_paymentStatusResponseDto);

            }

            // Act
            Func<Task> action = async () => await _paymentsService.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }
    }
}
