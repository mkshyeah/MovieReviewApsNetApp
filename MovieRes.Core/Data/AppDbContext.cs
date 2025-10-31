using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieRes.Core.Features.Reviews;

namespace MovieRes.Core.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    
    // DbSet'ы для основных сущностей
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Review> Reviews { get; set; }
    
    // DbSet'ы для новых сущностей
    public DbSet<Actor> Actors { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<ReviewLike> ReviewLikes { get; set; }
    
    //  DbSet'ы для связующих таблиц
    public DbSet<MovieActor> MovieActors { get; set; }
    public DbSet<MovieGenre> MovieGenres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Всегда вызывайте base.OnModelCreating для IdentityDbContext!

        // Установка схемы по умолчанию
        modelBuilder.HasDefaultSchema("MovieRev");

        // Автоматически находит и применяет все классы IEntityTypeConfiguration
        // (т.к MovieGenreConfiguration.cs) в этой сборке.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
    }
}