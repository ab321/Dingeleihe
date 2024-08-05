using FluentValidation;

namespace Core.Entities.Validation;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.DateOfBirth)
            .Must(BeValidAge).WithMessage("Der Benutzer muss mindestens 15 Jahre alt sein.");
    }
    
    private bool BeValidAge(DateTime? dateOfBirth)
    {
        if(dateOfBirth == null)
            return true;
        
        return dateOfBirth.Value.AddYears(15) <= DateTime.Now;
    }
}