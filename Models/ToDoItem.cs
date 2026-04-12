namespace webapi_demo.Models;

public class ToDoItem
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public bool IsComplete { get; set; }
}
