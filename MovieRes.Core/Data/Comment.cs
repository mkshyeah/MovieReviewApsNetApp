namespace MovieRes.Core.Data;

public class Comment
{
    public int Id { get; set; }
    
    public required string Text { get; set; }
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    
    // Внешние ключи
    public required string UserId { get; set; }
    public required int ReviewId { get; set; }
    
    // Навигационные свойства
    public ApplicationUser User { get; set; } = default!;
    public Review Review { get; set; } = default!;
}