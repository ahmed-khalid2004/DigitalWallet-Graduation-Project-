using AutoMapper;
using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.DTOs.Wallet;
using DigitalWallet.Application.DTOs.Transfer;
using DigitalWallet.Application.DTOs.Transaction;
using DigitalWallet.Application.DTOs.Notification;
using DigitalWallet.Application.DTOs.BillPayment;
using DigitalWallet.Application.DTOs.MoneyRequest;
using DigitalWallet.Application.DTOs.Admin;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<User, UserDto>();

            // Wallet
            CreateMap<Wallet, WalletDto>();
            CreateMap<Wallet, WalletBalanceDto>();

            // Transaction
            CreateMap<Transaction, TransactionDto>();

            // Transfer
            CreateMap<Transfer, TransferDto>();

            // MoneyRequest
            CreateMap<MoneyRequest, MoneyRequestDto>()
                .ForMember(dest => dest.FromUserName, opt => opt.MapFrom(src => src.FromUser.FullName))
                .ForMember(dest => dest.ToUserName, opt => opt.MapFrom(src => src.ToUser.FullName));

            // BillPayment
            CreateMap<BillPayment, BillPaymentDto>()
                .ForMember(dest => dest.BillerName, opt => opt.MapFrom(src => src.Biller.Name));
            CreateMap<Biller, BillerDto>();

            // Notification
            CreateMap<Notification, NotificationDto>();

            // Admin
            CreateMap<User, UserManagementDto>();
            CreateMap<FraudLog, FraudLogDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));
        }
    }
}