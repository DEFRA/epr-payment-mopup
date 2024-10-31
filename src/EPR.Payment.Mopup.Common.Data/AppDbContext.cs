using EPR.Payment.Mopup.Common.Data.DataModels.Lookups;
using EPR.Payment.Mopup.Common.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Mopup.Common.Data
{
    [ExcludeFromCodeCoverage]
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<PaymentStatus> PaymentStatus => Set<PaymentStatus>();
        public DbSet<DataModels.Payment> Payment => Set<DataModels.Payment>();
        public DbSet<DataModels.OnlinePayment> OnlinePayment => Set<DataModels.OnlinePayment>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer();
        }
    }
}
