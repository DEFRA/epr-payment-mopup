using AutoFixture.MSTest;
using EPR.Payment.Mopup.Common.Data.Interfaces;
using EPR.Payment.Mopup.Common.Data.Repositories;
using EPR.Payment.Mopup.Common.Enums;
using EPR.Payment.Mopup.Common.UnitTests.Mocks;
using EPR.Payment.Mopup.Common.UnitTests.TestHelpers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using System.Data.Entity;

namespace EPR.Payment.Mopup.Common.Data.UnitTests.Repositories
{
    [TestClass]
    public class PaymentsRepositoryTests
    {
        private Mock<DbSet<Common.Data.DataModels.OnlinePayment>> _paymentMock = null!;
        private Mock<IConfiguration> _configurationMock = null!;
        private CancellationToken _cancellationToken;

        [TestInitialize]
        public void TestInitialize()
        {
            _paymentMock = MockIPaymentRepository.GetPaymentMock();
            _configurationMock = new Mock<IConfiguration>();
            _cancellationToken = new CancellationToken();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenAllDependenciesAreNotNull_ShouldCreateInstance(
             [Frozen] Mock<IAppDbContext> _dataContextMock,
             [Frozen] Mock<IConfiguration> _configurationMock
            )
        {
            // Act
            var repo = new PaymentsRepository(
                _dataContextMock.Object, 
                _configurationMock.Object);

            // Assert
            repo.Should().NotBeNull();
        }

        [TestMethod, AutoMoqData]
        public void Constructor_WhenDataContextIsNull_ShouldThrowArgumentNullException(
              [Frozen] Mock<IConfiguration> _configurationMock
              )
        {
            // Act
            Action act = () => new PaymentsRepository(
                null!,
                _configurationMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("dataContext");
        }


        [TestMethod, AutoMoqData]
        public void Constructor_WhenConfigurationMockIsNull_ShouldThrowArgumentNullException(
                [Frozen] Mock<IAppDbContext> _dataContextMock
              )
        {
            // Act
            Action act = () => new PaymentsRepository(
                 _dataContextMock.Object,
                null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentStatusAsync_ValidInput_ShouldComplete(
            [Frozen] Mock<IAppDbContext> _dataContextMock,
            [Greedy] PaymentsRepository _mockPaymentsRepository,
            [Frozen] Guid newId,
            [Frozen] Guid userId,
            [Frozen] Guid organisationId)
        {
            //Arrange
            _dataContextMock.Setup(i => i.Payment).ReturnsDbSet(_paymentMock.Object);
            _mockPaymentsRepository = new PaymentsRepository(_dataContextMock.Object, _configurationMock.Object);

            var request = new Common.Data.DataModels.OnlinePayment
            {
                ExternalPaymentId = newId,
                UserId = userId,
                OrganisationId = organisationId,
            };

            //Act
            await _mockPaymentsRepository.UpdatePaymentStatusAsync(request, _cancellationToken);


            //Assert
            using (new AssertionScope())
            {
                _dataContextMock.Verify(c => c.Payment.Update(It.Is<Common.Data.DataModels.OnlinePayment>(s => s.UserId == userId && s.OrganisationId == organisationId)), Times.Once());
                _dataContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(1));
                request.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
                request.GovPayStatus.Should().Be(Enum.GetName(typeof(Enums.Status), request.InternalStatusId));
            }
        }

        [TestMethod, AutoMoqData]
        public async Task UpdatePaymentStatusAsync_NullEntity_ShouldThrowArgumentException(
            [Frozen] Mock<IAppDbContext> _dataContextMock,
            [Greedy] PaymentsRepository _mockPaymentsRepository)
        {
            //Arrange
            _dataContextMock.Setup(i => i.Payment).ReturnsDbSet(_paymentMock.Object);
            _mockPaymentsRepository = new PaymentsRepository(_dataContextMock.Object, _configurationMock.Object);


            Common.Data.DataModels.OnlinePayment? request = null;


            //Act & Assert
            await _mockPaymentsRepository.Invoking(async x => await x.UpdatePaymentStatusAsync(request, _cancellationToken))
                .Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod,AutoMoqData]
        public async Task GetPaymentsByStatusAsync_PaymentsExist_ShouldReturnPayments(
            [Frozen] Mock<IAppDbContext> _dataContextMock,
            [Greedy] PaymentsRepository _mockPaymentsRepository)
        {
            //Arrange
            DateTime dateTime = DateTime.UtcNow;
            int TotalMinutesToUpdate = 30;
            int IgnoringMinutesToUpdate = 15;

            _configurationMock.Setup(x => x["TotalMinutesToUpdate"]).Returns(TotalMinutesToUpdate.ToString());
            _configurationMock.Setup(x => x["IgnoringMinutesToUpdate"]).Returns(IgnoringMinutesToUpdate.ToString());

            var ignoringFrom = dateTime.AddMinutes(-IgnoringMinutesToUpdate);

            _dataContextMock.Setup(i => i.Payment).ReturnsDbSet(_paymentMock.Object);
            _mockPaymentsRepository = new PaymentsRepository(_dataContextMock.Object, _configurationMock.Object);

            //Act
            var result = await _mockPaymentsRepository.GetPaymentsByStatusAsync(Status.InProgress, _cancellationToken);

            //Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result[0].InternalStatusId.Should().Be(Status.InProgress);
                result[0].CreatedDate.Should().BeCloseTo(ignoringFrom.AddMinutes(-1), TimeSpan.FromSeconds(1));
            }
        }
    }
}
