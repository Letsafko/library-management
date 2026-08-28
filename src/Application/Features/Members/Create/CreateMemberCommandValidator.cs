using Domain.Members;
using Domain.Members.ValueObjects;
using FluentValidation;

namespace Application.Features.Members.Create;

public sealed class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .AddError(MemberErrors.MissingFirstName);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .AddError(MemberErrors.MissingLastName);

        RuleFor(x => x.Email)
            .NotEmpty()
            .AddError(MemberErrors.MissingEmail);

        RuleFor(x => x.MembershipType)
            .NotEmpty()
            .AddError(MemberErrors.MissingMembershipType);

        RuleFor(x => x.MembershipType)
            .Must(name => MembershipType.IsValid(name))
            .When(x => !string.IsNullOrEmpty(x.MembershipType))
            .AddError(MemberErrors.InvalidMembershipType);
    }
}
