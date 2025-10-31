using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieRes.Core.Data.Configurations;

public class MovieActorConfiguration : IEntityTypeConfiguration<MovieActor>
{
    public void Configure(EntityTypeBuilder<MovieActor> builder)
    {
        // Композитный ключ
        builder.HasKey(ma => new { ma.MovieId, ma.ActorId });
        
        // Дополнительные настройки для поля Role
        builder.Property(ma => ma.Role).HasMaxLength(100);
    }
}