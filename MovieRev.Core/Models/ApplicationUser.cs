using Microsoft.AspNetCore.Identity;
using MovieRev.Core.Data;

namespace MovieRev.Core.Models;

public sealed class ApplicationUser : IdentityUser
{
    public bool EnableNotifications { get; set; }
    public string Initials {get; set;} = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<ReviewLike> ReviewLikes { get; set; } = new List<ReviewLike>();
}