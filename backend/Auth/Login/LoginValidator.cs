using FastEndpoints;
using FluentValidation;

namespace low_cost_flight.Auth.Login;

public class LoginValidator : Validator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email è obbligatoria.")
            .EmailAddress().WithMessage("Formato email non valido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La password è obbligatoria.");
    }
}
