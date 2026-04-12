using Microsoft.AspNetCore.JsonPatch;
using webapi_demo.Models;

namespace webapi_demo.Interfaces;

public interface IToDoService
{
    Task<IEnumerable<ToDoItem>> GetAllAsync();

    Task<ToDoItem?> GetByIdAsync(int id);

    Task<ToDoItem> AddAsync(ToDoItem newItem);

    Task<ToDoItem?> UpdateAsync(int id, ToDoItem updatedItem);

    Task PatchAsync(int id, JsonPatchDocument<ToDoItem> patchDoc);

    Task<ToDoItem?> DeleteAsync(int id);
}
