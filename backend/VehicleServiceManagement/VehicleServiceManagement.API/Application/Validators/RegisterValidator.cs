using FluentValidation;
using VehicleServiceManagement.API.Application.DTOs.Auth;

namespace VehicleServiceManagement.API.Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.FullName).NotEmpty();
            RuleFor(x => x.Email).EmailAddress().NotEmpty();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8).WithMessage("Minimum 8 characters required.")
                .Matches(@"[A-Z]").WithMessage("Must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Must contain at least one number.")
                .Matches(@"[!@#$%^&*()_+=\[\]{};:'"",.<>/?\\|`~\-]").WithMessage("Must contain at least one special character.");
        }
    }
}