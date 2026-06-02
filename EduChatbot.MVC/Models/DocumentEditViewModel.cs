using System.ComponentModel.DataAnnotations;

namespace EduChatbot.MVC.Models;

public class DocumentEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter the file name.")]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the lecturer name.")]
    [MaxLength(100)]
    public string UploadedBy { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a status.")]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;
}
