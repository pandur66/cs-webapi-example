
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using webapi_demo.Data;
using webapi_demo.Interfaces;
using webapi_demo.Models;

namespace webapi_demo.Services;
public class ToDoService : IToDoService
{
    private readonly ToDoDbContext _context;
    private readonly ILogger<ToDoService> _logger;

    public ToDoService(ToDoDbContext context, ILogger<ToDoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ToDoItem> AddAsync(ToDoItem newItem)
    {
        await _context.ToDoItems.AddAsync(newItem);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created ToDoItem {Id}", newItem.Id);
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
        if (item == null)
        {
            _logger.LogWarning("PatchAsync: ToDoItem {Id} not found", id);
            return;
        }

        patchDoc.ApplyTo(item);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Patched ToDoItem {Id}", id);
    }

    public async Task<ToDoItem?> DeleteAsync(int id)
    {
        var item = await _context.ToDoItems.FindAsync(id);
        if (item == null)
        {
            _logger.LogWarning("DeleteAsync: ToDoItem {Id} not found", id);
            return null;
        }

        _context.ToDoItems.Remove(item);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted ToDoItem {Id}", id);
        return item;
    }

    public async Task<ToDoItem?> UpdateAsync(int id, ToDoItem updatedItem)
    {
        var existingItem = await _context.ToDoItems.FindAsync(id);
        if (existingItem == null)
        {
            _logger.LogWarning("UpdateAsync: ToDoItem {Id} not found", id);
            return null;
        }

        existingItem.Title = updatedItem.Title;
        existingItem.IsComplete = updatedItem.IsComplete;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated ToDoItem {Id}", id);
        return existingItem;
    }
}
