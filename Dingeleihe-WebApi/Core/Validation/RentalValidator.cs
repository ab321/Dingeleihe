using FluentValidation;

namespace Core.Entities.Validation;

public class RentalValidator : AbstractValidator<Rental>
{
    public RentalValidator()
    {
        RuleFor(x => x.User.DateOfBirth)
            .Must(((x, dob) => BeValidAgeRestriction(dob, x.Thing.ThingDetails.AgeRestriction)))
            .WithMessage("Der Benutzer ist nicht alt genug, um dieses Ding auszuleihen.");
    }
    
    private bool BeValidAgeRestriction(DateTime? dob, int ageRestriction)
    {
        
        return dob.Value.AddYears(ageRestriction) <= DateTime.Today;
    }
}