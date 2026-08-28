using Application.Messaging;

namespace Application.Features.Members.GetPenalties;

public sealed record GetMemberPenaltiesQuery(int MemberId) : IQuery;
