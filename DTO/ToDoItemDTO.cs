using System.ComponentModel.DataAnnotations;

namespace webapi_demo.DTO;

public class ToDoItemDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
    public string Title { get; set; } = null!;

    public bool IsComplete { get; set; }
}
