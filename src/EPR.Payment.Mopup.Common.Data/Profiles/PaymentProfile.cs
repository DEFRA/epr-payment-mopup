using AutoMapper;
using EPR.Payment.Mopup.Common.Dtos.Request;

namespace EPR.Payment.Mopup.Common.Data.Profiles
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<DataModels.OnlinePayment, PaymentDto>().ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.InternalStatusId));

            CreateMap<UpdatePaymentRequestDto, DataModels.OnlinePayment>().ForMember(dest => dest.InternalStatusId, opt => opt.MapFrom(src => src.Status));

            CreateMap<UpdatePaymentRequestDto, PaymentDto>().ReverseMap();
        }
    }
}
