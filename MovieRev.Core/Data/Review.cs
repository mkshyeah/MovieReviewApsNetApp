namespace MovieRev.Core.Data;

public class Review
{
    public int Id { get; set; }
    
    public required decimal Rating { get; set; }
    public required string Text { get; set; } = string.Empty;
    
    public DateTimeOffset ReviewDate { get; set; } = DateTimeOffset.UtcNow; 
    
    public required bool IsSpoiler { get; set; }
    
    // Счетчик лайков для TODO
    public int LikesCount { get; set; }
    
    // Внешние ключи
    public required string UserId { get; set; }
    public required int MovieId { get; set; }
    
    // Навигационные свойства
    public ApplicationUser User { get; set; } = default!; // Добавим default! для non-nullable
    public Movie Movie { get; set; } = default!; // Добавим default! для non-nullable
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<ReviewLike> Likes { get; set; } = new List<ReviewLike>();
}