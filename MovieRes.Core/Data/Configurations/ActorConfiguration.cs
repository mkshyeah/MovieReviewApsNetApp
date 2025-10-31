using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieRes.Core.Data.Configurations;

public class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        // Npgsql/PostgreSQL корректно работает с System.DateOnly
        builder.Property(a => a.DateOfBirth).HasColumnType("date");
        
        // Связи актера с фильмами
        builder.HasMany(a => a.MovieActors)
            .WithOne(ma => ma.Actor)
            .HasForeignKey(ma => ma.ActorId)
            .OnDelete(DeleteBehavior.Restrict);// Актера можно удалить независимо от фильмов (защита)
    }
}