using EPR.Payment.Mopup.Common.Constants;
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
        public void GetPaymentStatus_InvalidStatus_ThrowsException()
        {
            // Arrange
            string invalidStatus = "invalid_status";
            string errorCode = "P0010";

            // Act
            Action act = () => PaymentStatusMapper.GetPaymentStatus(invalidStatus, errorCode);

            // Assert
            act.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
        }

        [TestMethod]
        public void GetPaymentStatus_SuccessStatus_ReturnsSuccess()
        {
            // Arrange
            string status = "success";

            // Act
            var resultWithNullErrorCode = PaymentStatusMapper.GetPaymentStatus(status, null);
            var resultWithEmptyErrorCode = PaymentStatusMapper.GetPaymentStatus(status, string.Empty);

            // Assert
            resultWithNullErrorCode.Should().Be(Status.Success);
            resultWithEmptyErrorCode.Should().Be(Status.Success);
        }

        [TestMethod]
        public void GetPaymentStatus_SuccessStatusWithErrorCode_ThrowsException()
        {
            // Arrange
            string status = "success";
            string errorCode = "P0030";


            // Act & Assert
            Action actNull = () => PaymentStatusMapper.GetPaymentStatus(status, errorCode);
            actNull.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.SuccessStatusWithErrorCode);
        }

        [TestMethod]
        public void GetPaymentStatus_FailedStatusWithP0030_ReturnsUserCancelled()
        {
            // Arrange
            string status = "failed";
            string errorCode = "P0030";

            // Act
            var result = PaymentStatusMapper.GetPaymentStatus(status, errorCode);

            // Assert
            result.Should().Be(Status.UserCancelled);
        }

        [TestMethod]
        public void GetPaymentStatus_FailedStatusWithOtherCode_ReturnsFailed()
        {
            // Arrange
            string status = "failed";
            string errorCode = "P0010";

            // Act
            var result = PaymentStatusMapper.GetPaymentStatus(status, errorCode);

            // Assert
            result.Should().Be(Status.Failed);
        }

        [TestMethod]
        public void GetPaymentStatus_InvalidErrorCodeForFailedStatus_ThrowsException()
        {
            // Arrange
            string status = "failed";

            // Act & Assert
            Action actNull = () => PaymentStatusMapper.GetPaymentStatus(status, null);
            actNull.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.FailedStatusWithoutErrorCode);

            Action actEmpty = () => PaymentStatusMapper.GetPaymentStatus(status, string.Empty);
            actEmpty.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.FailedStatusWithoutErrorCode);
        }

        [TestMethod]
        public void GetPaymentStatus_ErrorStatusWithCode_ReturnsError()
        {
            // Arrange
            string status = "error";
            string errorCode = "P0050";

            // Act
            var result = PaymentStatusMapper.GetPaymentStatus(status, errorCode);

            // Assert
            result.Should().Be(Status.Error);
        }

        [TestMethod]
        public void GetPaymentStatus_InvalidErrorCodeForErrorStatus_ThrowsException()
        {
            // Arrange
            string status = "error";

            // Act & Assert
            Action actNull = () => PaymentStatusMapper.GetPaymentStatus(status, null);
            actNull.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.ErrorStatusWithoutErrorCode);

            Action actEmpty = () => PaymentStatusMapper.GetPaymentStatus(status, string.Empty);
            actEmpty.Should().Throw<ServiceException>().WithMessage(ExceptionMessages.ErrorStatusWithoutErrorCode);
        }
    }

}
