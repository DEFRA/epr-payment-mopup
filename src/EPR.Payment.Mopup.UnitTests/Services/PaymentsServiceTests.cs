using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Common.Data.Interfaces.Repositories;
using EPR.Payment.Mopup.Common.Data.Profiles;
using EPR.Payment.Mopup.Common.Dtos.Response;
using EPR.Payment.Mopup.Common.Dtos.Response.Common;
using EPR.Payment.Mopup.Common.Exceptions;
using EPR.Payment.Mopup.Common.RESTServices.Interfaces;
using EPR.Payment.Mopup.Common.UnitTests.TestHelpers;
using EPR.Payment.Mopup.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Mopup.UnitTests.Services
{
    [TestClass]
    public class PaymentsServiceTests
    {
        private IFixture _fixture = null!;
        private Mock<IPaymentsRepository> _paymentsRepositoryMock = null!;
        private Mock<IHttpGovPayService> _httpGovPayServiceMock = null!;
        private Mock<ILogger<PaymentsService>> _loggerMock = null!;
        private IMapper _mapper = null!;
        private PaymentsService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            var throwingRecursionBehaviors = _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList();
            foreach (var behavior in throwingRecursionBehaviors)
            {
                _fixture.Behaviors.Remove(behavior);
            }
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _paymentsRepositoryMock = _fixture.Freeze<Mock<IPaymentsRepository>>();
            _httpGovPayServiceMock = _fixture.Freeze<Mock<IHttpGovPayService>>();
            _loggerMock = _fixture.Freeze<Mock<ILogger<PaymentsService>>>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _service = new PaymentsService(
                _paymentsRepositoryMock.Object,
                _httpGovPayServiceMock.Object,
                _loggerMock.Object,
                _mapper
                );
        }

        [TestMethod]
        [DataRow("u4g456vpdeamkg72ihjt129io6", "success", true, null, null)]
        [DataRow("ubs761va12akse548htbu2oie5", "failed", true, "Payment method rejected", "P0010")]
        [DataRow("mpni2094aqjf89q5k2rdveed2o", "failed", true, "Payment method rejected", "P0010")]
        [DataRow("ke41o2t96h1983os203no7q5so", "failed", true, "Payment method rejected", "P0010")]
        [DataRow("jgpldh7b1i5qmh59ru6ia67386", "error", true, "Payment provider returned an error", "P0050")]
        [DataRow("n9nvasa4782ogtuh19e8mum68r", "failed", true, "Payment was cancelled by the user", "P0030")]

        public async Task UpdatePaymentsAsync_ShouldUpdatePayment_WhenPaymentExists(
            string govPayPaymentId,
            string status,
            bool finished,
            string message,
            string code)
        {
            // Arrange
            List<Common.Data.DataModels.Payment> payments = _fixture.Create<List<Common.Data.DataModels.Payment>>();
            payments.ForEach(s => s.GovpayPaymentId = govPayPaymentId);

            _paymentsRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Common.Enums.Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync( payments );


            var paymentStatusResponse = _fixture.Create<PaymentStatusResponseDto>();

            paymentStatusResponse.PaymentId = govPayPaymentId;
            paymentStatusResponse.State = new State { Status = status, Finished = finished, Message = message, Code = code };

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentStatusResponse);


            // Act
            await _service.UpdatePaymentsAsync(new CancellationToken());

            // Asserts
            _paymentsRepositoryMock.Verify(repo => repo.UpdatePaymentStatusAsync(It.IsAny<Common.Data.DataModels.Payment>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenPaymentIdIsNull()
        {
            // Arrange
            List<Common.Data.DataModels.Payment> payments = _fixture.Create<List<Common.Data.DataModels.Payment>>();
            payments.ForEach(s => s.GovpayPaymentId = null);

            _paymentsRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Common.Enums.Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payments);

            // Act
            Func<Task> action = async () => await _service.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage(ExceptionMessages.PaymentIdNotFound);

        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentsAsync_ShouldThrowException_WhenPaymentStatusIsNull(string govPayPaymentId)
        {
            // Arrange
            List<Common.Data.DataModels.Payment> payments = _fixture.Create<List<Common.Data.DataModels.Payment>>();
            payments.ForEach(s => s.GovpayPaymentId = govPayPaymentId);

            _paymentsRepositoryMock
                .Setup(repo => repo.GetPaymentsByStatusAsync(Common.Enums.Status.InProgress, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payments);

            _httpGovPayServiceMock.Setup(s => s.GetPaymentStatusAsync(govPayPaymentId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PaymentStatusResponseDto?)null);

            // Act
            Func<Task> action = async () => await _service.UpdatePaymentsAsync(new CancellationToken());

            // Assert
            await action.Should().ThrowAsync<ServiceException>()
                        .WithMessage(ExceptionMessages.ErrorRetrievingPaymentStatus);

        }
    }
}
