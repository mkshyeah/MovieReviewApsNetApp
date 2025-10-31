namespace MovieRes.Core.Data;

public class MovieGenre
{
    public int MovieId { get; set; }
    public int GenreId { get; set; }
    
    // Навигационные свойства
    public Movie Movie { get; set; } = default!;
    public Genre Genre { get; set; } = default!;
}