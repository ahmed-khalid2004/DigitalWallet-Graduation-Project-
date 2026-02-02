using DigitalWallet.Application.DTOs.Transfer;
using FluentValidation;

namespace DigitalWallet.Application.Validators
{
    public class SendMoneyRequestValidator : AbstractValidator<SendMoneyRequestDto>
    {
        public SendMoneyRequestValidator()
        {
            RuleFor(x => x.SenderWalletId)
                .NotEmpty().WithMessage("Sender wallet is required");

            RuleFor(x => x.ReceiverPhoneOrEmail)
                .NotEmpty().WithMessage("Receiver is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero")
                .LessThanOrEqualTo(50000).WithMessage("Amount cannot exceed 50,000");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP is required")
                .Length(6).WithMessage("OTP must be 6 digits");
        }
    }
}