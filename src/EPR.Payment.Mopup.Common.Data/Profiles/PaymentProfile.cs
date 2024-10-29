using AutoMapper;
using EPR.Payment.Mopup.Common.Data.DataModels;
using EPR.Payment.Mopup.Common.Dtos.Request;

namespace EPR.Payment.Mopup.Common.Data.Profiles
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<DataModels.Payment, PaymentDto>().ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.InternalStatusId))
                                           .ForMember(dest => dest.GovPayPaymentId, opt => opt.MapFrom(src => src.OnlinePayment.GovPayPaymentId));

            CreateMap<UpdatePaymentRequestDto, DataModels.Payment>().ForMember(dest => dest.InternalStatusId, opt => opt.MapFrom(src => src.Status));

            CreateMap<UpdatePaymentRequestDto, OnlinePayment>().ReverseMap();

            CreateMap<UpdatePaymentRequestDto, PaymentDto>().ReverseMap();


        }
    }
}
