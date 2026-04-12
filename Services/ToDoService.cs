
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using webapi_demo.Data;
using webapi_demo.Interfaces;
using webapi_demo.Models;

namespace webapi_demo.Services;
public class ToDoService : IToDoService
{
    private readonly ToDoDbContext _context;

    public ToDoService(ToDoDbContext context)
    {
        _context = context;
    }

    public async Task<ToDoItem> AddAsync(ToDoItem newItem)
    {
        await _context.ToDoItems.AddAsync(newItem);
        await _context.SaveChangesAsync();
        return newItem;
    }

    public async Task<IEnumerable<ToDoItem>> GetAllAsync()
    {
        return await _context.ToDoItems.AsNoTracking().ToListAsync();
    }

    public Task<ToDoItem?> GetByIdAsync(int id)
    {
        return _context.ToDoItems.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
    }

  public async Task PatchAsync(int id, JsonPatchDocument<ToDoItem> patchDoc)
    {
        var item = await _context.ToDoItems.FindAsync(id);
        if (item == null) return;
        
        patchDoc.ApplyTo(item);
        await _context.SaveChangesAsync();
    }

    public async Task<ToDoItem?> DeleteAsync(int id)
    {
        var item = await _context.ToDoItems.FindAsync(id);
        if (item == null) return null;

        _context.ToDoItems.Remove(item);
        await _context.SaveChangesAsync();
        return item;
    }  

    public async Task<ToDoItem?> UpdateAsync(int id, ToDoItem updatedItem)
    {
        var existingItem = await _context.ToDoItems.FindAsync(id);
        if (existingItem == null) return null;

        existingItem.Title = updatedItem.Title;
        existingItem.IsComplete = updatedItem.IsComplete;

        await _context.SaveChangesAsync();
        return existingItem;
    }
}
