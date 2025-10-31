using MovieRev.Core.Settings; // Добавляем using для TMDbSettings
using Microsoft.Extensions.Options;
using MovieRev.Core.Models.TMDb; // Добавляем для IOptions<TMDbSettings>

namespace MovieRev.Core.Services;

public class TMDbService
{
    private readonly HttpClient _httpClient;
    private readonly TMDbSettings _settings;

    public TMDbService(HttpClient httpClient, IOptions<TMDbSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
    }

    public async Task<TMDbMovieSearchResult?> SearchMovies(string query, CancellationToken cancellationToken)
    {
        var url = $"search/movie?api_key={_settings.ApiKey}&query={Uri.EscapeDataString(query)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TMDbMovieSearchResult>(cancellationToken: cancellationToken);
    }

    public async Task<TMDbMovieDetail?> GetMovieDetails(int tmdbMovieId, CancellationToken cancellationToken)
    {
        var url = $"movie/{tmdbMovieId}?api_key={_settings.ApiKey}&append_to_response=credits,genres";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TMDbMovieDetail>(cancellationToken: cancellationToken);
    }
}
