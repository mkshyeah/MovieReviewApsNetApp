using System.ComponentModel.DataAnnotations;
using MovieRev.Core.Models;

namespace MovieRev.Core.Data;

public enum ProposalStatus
{
    Pending,
    Approved,
    Rejected
} 
public class MovieProposal
{
    public int Id { get; set; }
    
    // Информация о фильме, которую предлагает пользователь
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }
    public int? TMDbId { get; set; } // Пользователь может сразу указать ID с TMDb
    
    [MaxLength(500)]
    public string? Notes { get; set; } // Заметки от автора предложения
    
    // Статус модерации
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;
    
    // Кто и когда предложил
    [Required]
    public required string ProposedByUserId { get; set; }
    public ApplicationUser ProposedByUser { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Кто и когда обработал
    public string? ReviewedByUserId { get; set; }
    public ApplicationUser? ReviewedByUser { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    [MaxLength(500)]
    public string? RejectionReason { get; set; }// Причина отклонения
}