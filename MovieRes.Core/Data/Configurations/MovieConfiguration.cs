using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieRes.Core.Data.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        // Movie -> MovieActors (Актеры)
        builder.HasMany(m => m.MovieActors)
            .WithOne(ma => ma.Movie)
            .HasForeignKey(m => m.MovieId)
            .OnDelete(DeleteBehavior.Cascade); // При удалении фильма удаляются записи о его актерах
        
        // Movie -> MovieGenres (Жанры)
        builder.HasMany(m => m.MovieGenres)
            .WithOne(ma => ma.Movie)
            .HasForeignKey(m => m.MovieId)
            .OnDelete(DeleteBehavior.Cascade); // При удалении фильма удаляются записи о его жанрах
    }
}