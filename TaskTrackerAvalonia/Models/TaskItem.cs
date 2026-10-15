using System;
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAvalonia.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required(ErrorMessage = "Введите название задачи.")]
    [MaxLength(100, ErrorMessage = "Название не должно превышать 100 символов.")]
    public string Title { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; }
}