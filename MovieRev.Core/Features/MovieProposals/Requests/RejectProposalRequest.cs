using System.ComponentModel.DataAnnotations;

namespace MovieRev.Core.Features.MovieProposals;

public record RejectProposalRequest(
    [Required] string Reason
);