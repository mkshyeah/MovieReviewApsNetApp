using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieRes.Core.Data.Configurations;

public class ReviewLikeConfiguration : IEntityTypeConfiguration<ReviewLike>
{
    public void Configure(EntityTypeBuilder<ReviewLike> builder)
    {
        // Композитный ключ (кто поставил лайк, какому отзыву)
        builder.HasKey(rl => new { rl.UserId, rl.ReviewId }); 
        
        // Настройка связи с User (уже сделана в ApplicationUserConfiguration, но можно повторить)
        builder.HasOne(rl => rl.User)
            .WithMany(u => u.ReviewLikes)
            .HasForeignKey(rl => rl.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Если пользователь удален, его лайки удаляются
    }
}