using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Common.Dtos.Response;
using EPR.Payment.Mopup.Common.Enums;
using EPR.Payment.Mopup.Common.Exceptions;
using EPR.Payment.Mopup.Common.Mappers;
using FluentAssertions;

namespace EPR.Payment.Mopup.Common.UnitTests.Mappers
{
    [TestClass]
    public class PaymentStatusMapperTests
    {

        [TestMethod]
        public void GetPaymentStatus_StateIsNull_ThrowsException()
        {
            // Act
            Action act = () => PaymentStatusMapper.GetPaymentStatus((State?)null);

            // Assert
            act.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod]
        public void GetPaymentStatus_StatusIsNull_ThrowsException()
        {
            // Arrange
            var state = new State { Status = null };

            // Act
            Action act = () => PaymentStatusMapper.GetPaymentStatus(state);

            // Assert
            act.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod]
        public void GetPaymentStatus_InvalidStatus_ThrowsException()
        {
            // Arrange
            var state = new State { Status = "invalid_status", Code = "P0010" };

            // Act
            Action act = () => PaymentStatusMapper.GetPaymentStatus(state);

            // Assert
            act.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod]
        public void GetPaymentStatus_SuccessStatus_ReturnsSuccess()
        {
            // Act
            var resultWithNullErrorCode = PaymentStatusMapper.GetPaymentStatus(new State { Status = "success", Code = null });
            var resultWithEmptyErrorCode = PaymentStatusMapper.GetPaymentStatus(new State { Status = "success", Code = string.Empty });

            // Assert
            resultWithNullErrorCode.Should().Be(Status.Success);
            resultWithEmptyErrorCode.Should().Be(Status.Success);
        }

        [TestMethod]
        public void GetPaymentStatus_SuccessStatusWithErrorCode_ThrowsException()
        {
            // Act & Assert
            Action actNull = () => PaymentStatusMapper.GetPaymentStatus(new State { Status = "success", Code = "P0030" });
            actNull.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.SuccessStatusWithErrorCode);
        }

        [TestMethod]
        public void GetPaymentStatus_FailedStatusWithP0030_ReturnsUserCancelled()
        {
            // Act
            var result = PaymentStatusMapper.GetPaymentStatus(new State { Status = "failed", Code = "P0030" });

            // Assert
            result.Should().Be(Status.UserCancelled);
        }

        [TestMethod]
        public void GetPaymentStatus_FailedStatusWithOtherCode_ReturnsFailed()
        {
            // Act
            var result = PaymentStatusMapper.GetPaymentStatus(new State { Status = "failed", Code = "P0010" });

            // Assert
            result.Should().Be(Status.Failed);
        }

        [TestMethod]
        public void GetPaymentStatus_InvalidErrorCodeForFailedStatus_ThrowsException()
        {
            // Act & Assert
            Action actNull = () => PaymentStatusMapper.GetPaymentStatus(new State { Status = "failed", Code = null });
            actNull.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.FailedStatusWithoutErrorCode);

            Action actEmpty = () => PaymentStatusMapper.GetPaymentStatus(new State { Status = "failed", Code = string.Empty });
            actEmpty.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.FailedStatusWithoutErrorCode);
        }

        [TestMethod]
        public void GetPaymentStatus_ErrorStatusWithCode_ReturnsError()
        {
            // Act
            var result = PaymentStatusMapper.GetPaymentStatus(new State { Status = "error", Code = "P0050" });

            // Assert
            result.Should().Be(Status.Error);
        }

        [TestMethod]
        public void GetPaymentStatus_InvalidErrorCodeForErrorStatus_ThrowsException()
        {
            // Act & Assert
            Action actNull = () => PaymentStatusMapper.GetPaymentStatus(new State { Status = "error", Code = null });
            actNull.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.ErrorStatusWithoutErrorCode);

            Action actEmpty = () => PaymentStatusMapper.GetPaymentStatus(new State { Status = "error", Code = string.Empty });
            actEmpty.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.ErrorStatusWithoutErrorCode);
        }
    }

}
