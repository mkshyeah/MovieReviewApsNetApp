using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieRev.Core.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(e => e.EnableNotifications).HasDefaultValue(true);
        builder.Property(e => e.Initials).HasMaxLength(5);
        
        // User -> Reviews
        builder.HasMany(u => u.Reviews)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Удаление пользователя удаляет его отзывы
        
        // User -> Comments
        builder.HasMany(u => u.Comments)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Пользователь может быть удален, но комментарий останется (например, с пустым UserId)
        
        // User -> ReviewLike
        builder.HasMany(u => u.ReviewLikes)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Удаление пользователя удаляет его лайки
    }
}