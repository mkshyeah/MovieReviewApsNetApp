namespace MovieRes.Core.Data;

public class Movie
{
    public int Id { get; set; }
    
    public required string  Title { get; set; } = string.Empty;
    public required string OriginalTitle { get; set; } = string.Empty;
    public required string Director { get; set; } = string.Empty;
    public required int ReleaseYear { get; set; }    
    public required string Description { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public required int RuntimeMinutes { get; set; }

    //Установлено 0 для нового фильма
    public decimal AverageRating { get; set; } = 0.0m;
    public int ReviewCount { get; set; } = 0;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    // 🆕 Новые навигационные свойства для связей "многие ко многим"
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

}