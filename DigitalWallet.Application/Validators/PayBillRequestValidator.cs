using DigitalWallet.Application.DTOs.BillPayment;
using FluentValidation;

namespace DigitalWallet.Application.Validators
{
    public class PayBillRequestValidator : AbstractValidator<PayBillRequestDto>
    {
        public PayBillRequestValidator()
        {
            RuleFor(x => x.WalletId)
                .NotEmpty().WithMessage("Wallet is required");

            RuleFor(x => x.BillerId)
                .NotEmpty().WithMessage("Biller is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP is required")
                .Length(6).WithMessage("OTP must be 6 digits");
        }
    }
}