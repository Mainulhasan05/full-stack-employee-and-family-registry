using FluentValidation;
using family_registry_backend.DTOs;

namespace family_registry_backend.Validators;

public class EmployeeCreateDtoValidator : AbstractValidator<EmployeeCreateDto>
{
    public EmployeeCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.NID)
            .NotEmpty().WithMessage("NID is required.")
            .Must(nid => nid.Length == 10 || nid.Length == 17)
            .WithMessage("NID must be exactly 10 or 17 digits.")
            .Matches(@"^\d+$").WithMessage("NID must contain only digits.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Must(BeValidBDPhone).WithMessage("Phone must be a valid BD format (start with +880 or 01).");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.BasicSalary)
            .GreaterThan(0).WithMessage("Basic salary must be greater than 0.");

        When(x => x.Spouse != null, () =>
        {
            RuleFor(x => x.Spouse!.Name)
                .NotEmpty().WithMessage("Spouse name is required.");
            RuleFor(x => x.Spouse!.NID)
                .NotEmpty().WithMessage("Spouse NID is required.")
                .Must(nid => nid.Length == 10 || nid.Length == 17)
                .WithMessage("Spouse NID must be exactly 10 or 17 digits.")
                .Matches(@"^\d+$").WithMessage("Spouse NID must contain only digits.");
        });

        RuleForEach(x => x.Children).ChildRules(child =>
        {
            child.RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Child name is required.");
            child.RuleFor(c => c.DateOfBirth)
                .LessThan(DateTime.UtcNow).WithMessage("Child date of birth must be in the past.");
        });
    }

    private bool BeValidBDPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return false;

        // +880 format: +880XXXXXXXXXX (14 chars total)
        if (phone.StartsWith("+880"))
            return phone.Length >= 14 && phone[4..].All(char.IsDigit);

        // 01 format: 01XXXXXXXXX (11 chars total)
        if (phone.StartsWith("01"))
            return phone.Length == 11 && phone.All(char.IsDigit);

        return false;
    }
}

public class EmployeeUpdateDtoValidator : AbstractValidator<EmployeeUpdateDto>
{
    public EmployeeUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.NID)
            .NotEmpty().WithMessage("NID is required.")
            .Must(nid => nid.Length == 10 || nid.Length == 17)
            .WithMessage("NID must be exactly 10 or 17 digits.")
            .Matches(@"^\d+$").WithMessage("NID must contain only digits.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Must(BeValidBDPhone).WithMessage("Phone must be a valid BD format (start with +880 or 01).");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.BasicSalary)
            .GreaterThan(0).WithMessage("Basic salary must be greater than 0.");

        When(x => x.Spouse != null, () =>
        {
            RuleFor(x => x.Spouse!.Name)
                .NotEmpty().WithMessage("Spouse name is required.");
            RuleFor(x => x.Spouse!.NID)
                .NotEmpty().WithMessage("Spouse NID is required.")
                .Must(nid => nid.Length == 10 || nid.Length == 17)
                .WithMessage("Spouse NID must be exactly 10 or 17 digits.")
                .Matches(@"^\d+$").WithMessage("Spouse NID must contain only digits.");
        });

        RuleForEach(x => x.Children).ChildRules(child =>
        {
            child.RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Child name is required.");
            child.RuleFor(c => c.DateOfBirth)
                .LessThan(DateTime.UtcNow).WithMessage("Child date of birth must be in the past.");
        });
    }

    private bool BeValidBDPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return false;
        if (phone.StartsWith("+880"))
            return phone.Length >= 14 && phone[4..].All(char.IsDigit);
        if (phone.StartsWith("01"))
            return phone.Length == 11 && phone.All(char.IsDigit);
        return false;
    }
}
