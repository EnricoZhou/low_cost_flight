using FastEndpoints;
using FluentValidation;

namespace low_cost_flight.Auth.Register;

public class RegisterValidator : Validator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email è obbligatoria.")
            .EmailAddress().WithMessage("Formato email non valido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La password è obbligatoria.")
            .MinimumLength(6).WithMessage("La password deve contenere almeno 6 caratteri.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Il nome completo è obbligatorio.")
            .MaximumLength(150).WithMessage("Il nome non può superare 150 caratteri.");
    }
}
