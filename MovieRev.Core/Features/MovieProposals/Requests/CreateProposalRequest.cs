using System.ComponentModel.DataAnnotations;

namespace MovieRev.Core.Features.MovieProposals.Requests;

public record CreateProposalRequest(
    [Required] string Title,
    int? TMDbId,
    string? Notes);
