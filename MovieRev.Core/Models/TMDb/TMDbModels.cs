using System.Text.Json.Serialization;

namespace MovieRev.Core.Models.TMDb;

public class TMDbMovieSearchResult
{
    public int Page { get; set; }
    public List<TMDbMovieSummary> Results { get; set; } = new List<TMDbMovieSummary>();
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
}

public class TMDbMovieSummary
{
    public int Id { get; set; }
    public bool Adult { get; set; }
    
    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }
    public string? OriginalLanguage { get; set; }
    
    [JsonPropertyName("original_title")]
    public string? OriginalTitle { get; set; }
    public string? Overview { get; set; }
    public double Popularity { get; set; }
    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }
    
    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }
    public string? Title { get; set; }
    public bool Video { get; set; }
    public double VoteAverage { get; set; }
    public int VoteCount { get; set; }
}

public class TMDbMovieDetail : TMDbMovieSummary
{
    public string? Homepage { get; set; }
    public int Runtime { get; set; }
    public string? Status { get; set; }
    public string? Tagline { get; set; }
    public List<TMDbGenre> Genres { get; set; } = new List<TMDbGenre>();
    public TMDbCredits? Credits { get; set; }
}

public class TMDbGenre
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class TMDbCredits
{
    public List<TMDbCast> Cast { get; set; } = new List<TMDbCast>();
    public List<TMDbCrew> Crew { get; set; } = new List<TMDbCrew>();
}

public class TMDbCast
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Character { get; set; }
    public int Order { get; set; }
    
    [JsonPropertyName("profile_path")]
    public string? ProfilePath { get; set; }
}

public class TMDbCrew
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Job { get; set; }
    
    [JsonPropertyName("profile_path")]
    public string? ProfilePath { get; set; }
}
