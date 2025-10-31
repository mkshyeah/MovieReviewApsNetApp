using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieRev.Core.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        // Отношение Review (многие) к Movie (один)
        builder.HasOne(r => r.Movie)
            .WithMany(m => m.Reviews)
            .HasForeignKey(r => r.MovieId)
            .OnDelete(DeleteBehavior.Cascade);// Удаление фильма удаляет его отзывы (логично)
        
        // Отношение One-to-Many: Review -> Comments
        builder.HasMany(r => r.Comments)
            .WithOne(c => c.Review)
            .HasForeignKey(c => c.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);// Удаление отзыва удаляет все его комментарии

        
        // Отношение Many-to-Many: Review <-> ApplicationUser (через ReviewLike)
        builder.HasMany(r => r.Likes)
            .WithOne(l => l.Review)
            .HasForeignKey(l => l.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);// Удаление отзыва удаляет все связанные лайки
        
        
    }
}