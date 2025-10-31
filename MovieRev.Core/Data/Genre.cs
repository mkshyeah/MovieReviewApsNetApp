namespace MovieRev.Core.Data;

public class Genre
{
    public int Id { get; set; }
    
    public required string Name { get; set; } = string.Empty;
    
    // Навигационное свойство для связи "многие ко многим" с фильмами
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}