using Domain.Members;
using FluentValidation;

namespace Application.Features.Members.GetPenalties;

public sealed class GetMemberPenaltiesQueryValidator : AbstractValidator<GetMemberPenaltiesQuery>
{
    public GetMemberPenaltiesQueryValidator()
    {
        RuleFor(x => x.MemberId)
            .GreaterThan(0)
            .AddError(MemberErrors.InvalidMemberId);
    }
}
