﻿using EPR.Payment.Mopup.Common.UnitTests.TestHelpers;
using Moq;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace EPR.Payment.Mopup.Common.UnitTests.Mocks
{
    public static class MockIPaymentRepository
    {
        public static Mock<DbSet<Data.DataModels.Payment>> GetPaymentMock()
        {
            var paymentMock = new Mock<DbSet<Data.DataModels.Payment>>();


            var paymentMockData = new List<Data.DataModels.Payment>()
            {
              new Data.DataModels.Payment()
                {
                    Id = 1,
                    ExternalPaymentId = Guid.Parse("d0f74b07-42e1-43a7-ae9d-0e279f213278"),
                    UserId = Guid.NewGuid(),
                    Regulator = "Test 1 Regulator",
                    Reference = "Test 1 Reference",
                    InternalStatusId = Enums.Status.InProgress,
                    Amount = 10.0M,
                    ReasonForPayment = "Test 1",
                    CreatedDate = DateTime.UtcNow.AddMinutes(-16),
                    UpdatedByUserId = Guid.NewGuid(),
                    UpdatedDate = DateTime.UtcNow,
                    OnlinePayment = new Data.DataModels.OnlinePayment()
                    {
                        Id = 1,
                        PaymentId = 1,
                        UpdatedByOrgId = Guid.NewGuid(),
                        GovPayPaymentId = "123456",
                        OrganisationId = Guid.NewGuid()
                    }
                },
               new Data.DataModels.Payment()
                {
                    Id = 2,
                    ExternalPaymentId = Guid.Parse("dab3d8e1-409b-4b40-a610-1b41843e4710"),
                    UserId = Guid.NewGuid(),
                    Regulator = "Test 2 Regulator",
                    Reference = "Test 2 Reference",
                    InternalStatusId = Enums.Status.InProgress,
                    Amount = 10.0M,
                    ReasonForPayment = "Test 2",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedByUserId = Guid.NewGuid(),
                    UpdatedDate = DateTime.UtcNow,
                    OnlinePayment = new Data.DataModels.OnlinePayment()
                    {
                        Id = 2,
                        PaymentId = 2,
                        UpdatedByOrgId = Guid.NewGuid(),
                        GovPayPaymentId = "1256",
                        OrganisationId = Guid.NewGuid()
                    }
                }
            }.AsQueryable();

            paymentMock.As<IDbAsyncEnumerable<Data.DataModels.Payment>>()
            .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestHelperDbAsyncEnumerator<Data.DataModels.Payment>(paymentMockData.GetEnumerator()));

            paymentMock.As<IQueryable<Data.DataModels.Payment>>()
            .Setup(m => m.Provider)
                .Returns(new TestHelperDbAsyncQueryProvider<Data.DataModels.Payment>(paymentMockData.Provider));

            paymentMock.As<IQueryable<Data.DataModels.Payment>>().Setup(m => m.Expression).Returns(paymentMockData.Expression);
            paymentMock.As<IQueryable<Data.DataModels.Payment>>().Setup(m => m.ElementType).Returns(paymentMockData.ElementType);
            paymentMock.As<IQueryable<Data.DataModels.Payment>>().Setup(m => m.GetEnumerator()).Returns(() => paymentMockData.GetEnumerator());

            // Setup the mock
            return paymentMock;
        }
    }
}
