using AutoMapper;
using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Services.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();

            CreateMap<LoanAccount, LoanDto>();

            CreateMap<CreateLoanRequest, LoanAccount>();

            CreateMap<RepaymentSchedule, RepaymentScheduleDto>()
                .ForMember(
                    dest => dest.EmiAmount,
                    opt => opt.MapFrom(
                        src => src.Emiamount));

            CreateMap<PaymentBehaviorLog, PaymentBehaviorDto>();

            CreateMap<RiskPrediction, RiskPredictionDto>();

            CreateMap<BorrowerLoanApplication, BorrowerLoanApplicationDto>();
        }
    }
}