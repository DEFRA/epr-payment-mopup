using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Services.Interfaces;
using FluentAssertions.Execution;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Payment.Mopup.Function.UnitTests
{
    [TestClass]
    public class PaymentsFunctionTests
    {
        private Mock<IPaymentsService> _paymentsServiceMock = null!;
        private Mock<ILogger<PaymentsFunction>> _loggerMock = null!;
        private PaymentsFunction _paymentsFunction = null!;
        private CancellationToken _cancellationToken;

        [TestInitialize]
        public void TestInitialize()
        {
            _paymentsServiceMock = new Mock<IPaymentsService>();
            _loggerMock = new Mock<ILogger<PaymentsFunction>>();
            _paymentsFunction = new PaymentsFunction(_paymentsServiceMock.Object, _loggerMock.Object);
            _cancellationToken = new CancellationToken();
        }

        [TestMethod]
        public async Task Run_ShouldLogInformationAndUpdatePayment_Success()
        {
            //Arrange
            var timerInfo = CreateTimerInfo();

            // Act
            await _paymentsFunction.Run(timerInfo, _cancellationToken);

            //Assert
            using (new AssertionScope())
            {
                _paymentsServiceMock.Verify(x => x.UpdatePaymentsAsync(_cancellationToken), Times.Once);
                _loggerMock.Verify(
                    x => x.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Information),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mop Up time trigger function executed")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                        Times.Once);
            }
        }

        [TestMethod]
        public async Task Run_ShouldLogError_WhenUpdatePaymentThrowsException()
        {
            //Arrange
            var exception = new Exception("Test Exception");
            var timerInfo = CreateTimerInfo();

            _paymentsServiceMock.Setup(s => s.UpdatePaymentsAsync(_cancellationToken)).ThrowsAsync(exception);

            // Act
            await _paymentsFunction.Run(timerInfo, _cancellationToken);

            //Assert
            using (new AssertionScope())
            {
                _paymentsServiceMock.Verify(x => x.UpdatePaymentsAsync(_cancellationToken), Times.Once);
                _loggerMock.Verify(
                    x => x.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(LogMessages.ErrorOccured)),
                        It.Is<Exception>(e => e == exception),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                        Times.Once);
            }
        }

        private TimerInfo CreateTimerInfo()
        {
            var schedule = new DailySchedule();
            return new TimerInfo(schedule, new ScheduleStatus());
        }
    }
}
