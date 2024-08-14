using AutoMapper;
using EPR.Payment.Mopup.Common.Dtos.Request;

namespace EPR.Payment.Mopup.Common.Data.Profiles
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<PaymentDto, DataModels.Payment>().ReverseMap();
        }
    }
}
