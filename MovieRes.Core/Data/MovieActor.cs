namespace MovieRes.Core.Data;

public class MovieActor
{
    public  int MovieId { get; set; }
    public int ActorId { get; set; }
    
    // Навигационные свойства
    public Movie Movie { get; set; } = default!;
    public Actor Actor { get; set; } = default!;
    
    // Дополнительное поле: роль актера в фильме (например, "Железный человек")
    public string Role { get; set; } = string.Empty; 
}