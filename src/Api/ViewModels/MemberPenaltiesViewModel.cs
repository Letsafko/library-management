using Application.Features.Models;

namespace Api.ViewModels;

public sealed record MemberPenaltiesViewModel
{
    public MemberPenaltiesViewModel() { }

    public MemberPenaltiesViewModel(MemberPenaltiesResponse response)
    {
        MemberId = response.MemberId;
        TotalPendingPenalty = response.TotalPendingPenalty;
    }

    public int MemberId { get; init; }
    public decimal TotalPendingPenalty { get; init; }
}
