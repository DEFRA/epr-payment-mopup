using EPR.Payment.Mopup.Common.Constants;
using EPR.Payment.Mopup.Common.Data.Interfaces;
using EPR.Payment.Mopup.Common.Data.Interfaces.Repositories;
using EPR.Payment.Mopup.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EPR.Payment.Mopup.Common.Data.Repositories
{
    public class PaymentsRepository : IPaymentsRepository
    {
        private readonly IAppDbContext _dataContext;
        private readonly IConfiguration _configuration;
        public PaymentsRepository(
            IAppDbContext dataContext,
            IConfiguration configuration)
        {
            _dataContext = dataContext;
            _configuration = configuration;
        }

        public async Task UpdatePaymentStatusAsync(DataModels.Payment? entity, CancellationToken cancellationToken)
        {
            if (entity == null)
            {
                throw new ArgumentException(ExceptionMessages.InvalidInputToUpdatePaymentError);
            }

            entity.UpdatedDate = DateTime.Now;
            entity.GovPayStatus = Enum.GetName(typeof(Enums.Status), entity.InternalStatusId);
            _dataContext.Payment.Update(entity);
            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<DataModels.Payment>> GetPaymentsByStatusAsync(Status status, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            DateTime totalHoursToUpdate = now.AddHours(-Convert.ToInt32(_configuration["TotalHoursToUpdate"]));
            DateTime ignoringHoursToUpdate = now.AddHours(-Convert.ToInt32(_configuration["IgnoringHoursToUpdate"]));
            var entities = await _dataContext.Payment.Where(a => a.InternalStatusId == Status.InProgress && a.CreatedDate >= totalHoursToUpdate && a.CreatedDate <= ignoringHoursToUpdate).ToListAsync();
            return entities;
        }
    }
}
