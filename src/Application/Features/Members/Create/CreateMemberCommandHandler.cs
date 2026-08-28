using System.Threading;
using System.Threading.Tasks;
using Application.Features.Models;
using Application.Messaging;
using Domain.Members;
using Domain.Members.ValueObjects;
using SharedKernel.Primitives;

namespace Application.Features.Members.Create;

public sealed class CreateMemberCommandHandler(
    IMemberRepository memberRepository,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateMemberCommand, MemberResponse>
{
    public async Task<Result<MemberResponse>> HandleAsync(CreateMemberCommand? request, CancellationToken cancellationToken)
    {
        var membershipType = MembershipType.GetByName(request!.MembershipType!);
        var member = Member.Create(
            request.FirstName!,
            request.LastName!,
            request.Email!,
            membershipType,
            dateTimeProvider.UtcNow);

        await memberRepository.AddAsync(member, cancellationToken);
        return new MemberResponse(member);
    }
}
