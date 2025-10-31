namespace MovieRev.Core.Data;

public class Actor
{
    public int Id { get; set; }
    public required string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? PhotoUrl { get; set; }
    
    // Навигационное свойство для связи "многие ко многим" с фильмами
    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
}