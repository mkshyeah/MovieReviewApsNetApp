namespace MovieRev.Core.Data;

public class ReviewLike
{
    public required string UserId { get; set; }
    public required int ReviewId { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    
    // Навигационные свойства
    public ApplicationUser User { get; set; } = default!;
    public Review Review { get; set; } = default!;
}