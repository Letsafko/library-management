using System.Threading;
using System.Threading.Tasks;
using Application.Features.Models;
using Application.Messaging;
using Domain.Members;
using SharedKernel.Primitives;

namespace Application.Features.Members.GetPenalties;

public sealed class GetMemberPenaltiesQueryHandler(
    IMemberRepository memberRepository,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<GetMemberPenaltiesQuery, MemberPenaltiesResponse>
{
    public async Task<Result<MemberPenaltiesResponse>> HandleAsync(
        GetMemberPenaltiesQuery? request,
        CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request!.MemberId, cancellationToken);
        if (member is null)
        {
            return MemberErrors.MemberNotFound;
        }

        var totalPenalty = member.GetTotalPendingPenalties(dateTimeProvider.UtcNow);
        return new MemberPenaltiesResponse(request.MemberId, totalPenalty);
    }
}
