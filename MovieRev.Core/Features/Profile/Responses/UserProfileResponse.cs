namespace MovieRev.Core.Features.Profile.Responses;

public class UserProfileResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = [];
    public List<UserReviewSummary> Reviews { get; set; } = [];
    public List<UserProposalSummary> Proposals { get; set; } = [];
}

// ИЗМЕНЕНЫ ИМЕНА И ТИПЫ
public record UserReviewSummary(int Id, string MovieTitle, decimal Rating, string Text, DateTimeOffset ReviewDate);

// ИЗМЕНЕН ТИП ДАТЫ
public record UserProposalSummary(int Id, string MovieTitle, string Status, DateTimeOffset CreatedAt);