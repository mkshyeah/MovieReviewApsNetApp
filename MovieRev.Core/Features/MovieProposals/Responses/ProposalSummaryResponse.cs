namespace MovieRev.Core.Features.MovieProposals.Responses;

public record ProposalSummaryResponse(
    int Id,
    string Title,
    int? TMDbId,
    string Status,
    string ProposedByUserName,
    DateTimeOffset CreatedAt
);