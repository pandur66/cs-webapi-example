using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using webapi_demo.Data;
using webapi_demo.Models;
using webapi_demo.Services;
using Xunit;

namespace webapi_demo.Tests;

public class ToDoServiceTests : IDisposable
{
    private readonly ToDoDbContext _context;
    private readonly ToDoService _service;

    public ToDoServiceTests()
    {
        var options = new DbContextOptionsBuilder<ToDoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ToDoDbContext(options);
        _service = new ToDoService(_context, NullLogger<ToDoService>.Instance);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoItemsExist()
    {
        var result = await _service.GetAllAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllItems_WhenItemsExist()
    {
        var ct = TestContext.Current.CancellationToken;
        _context.ToDoItems.AddRange(
          new ToDoItem { Title = "First", IsComplete = false },
            new ToDoItem { Title = "Second", IsComplete = true }
        );
        await _context.SaveChangesAsync(ct);
        var result = await _service.GetAllAsync();
        Assert.Equal(2, result.Count());
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ReturnsItem_WhenItemExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new ToDoItem { Title = "Find me", IsComplete = false };
        _context.ToDoItems.Add(item);
        await _context.SaveChangesAsync(ct);

        var result = await _service.GetByIdAsync(item.Id);

        Assert.NotNull(result);
        Assert.Equal("Find me", result.Title);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        var result = await _service.GetByIdAsync(999);
        Assert.Null(result);
    }

    // AddAsync

    [Fact]
    public async Task AddAsync_ReturnsAddedItem()
    {
        var newItem = new ToDoItem { Title = "New Task", IsComplete = false };
        var result = await _service.AddAsync(newItem);
        Assert.NotNull(result);
        Assert.Equal("New Task", result.Title);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task AddAsync_PersistsItem_InDatabase()
    {
        var ct = TestContext.Current.CancellationToken;
        var newItem = new ToDoItem { Title = "Persisted Task", IsComplete = true };

        var result = await _service.AddAsync(newItem);

        Assert.True(result.Id > 0);
        Assert.Equal(1, await _context.ToDoItems.CountAsync(ct));
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        var result = await _service.UpdateAsync(999, new ToDoItem { Title = "Updated", IsComplete = true });
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedItem_WhenItemExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new ToDoItem { Title = "Original", IsComplete = false };
        _context.ToDoItems.Add(item);
        await _context.SaveChangesAsync(ct);
        var result = await _service.UpdateAsync(item.Id, new ToDoItem { Title = "Updated", IsComplete = true });
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Title);
        Assert.True(result.IsComplete);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges_InDatabase()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new ToDoItem { Title = "Original", IsComplete = false };
        _context.ToDoItems.Add(item);
        await _context.SaveChangesAsync(ct);

        await _service.UpdateAsync(item.Id, new ToDoItem { Title = "Persisted", IsComplete = true });

        var fromDb = await _context.ToDoItems.FindAsync([item.Id], ct);
        Assert.Equal("Persisted", fromDb!.Title);
        Assert.True(fromDb.IsComplete);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        var result = await _service.DeleteAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsDeletedItem_WhenItemExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new ToDoItem { Title = "To Delete", IsComplete = false };
        _context.ToDoItems.Add(item);
        await _context.SaveChangesAsync(ct);
        var result = await _service.DeleteAsync(item.Id);
        Assert.NotNull(result);
        Assert.Equal("To Delete", result.Title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem_FromDatabase()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new ToDoItem { Title = "To Delete", IsComplete = false };
        _context.ToDoItems.Add(item);
        await _context.SaveChangesAsync(ct);

        await _service.DeleteAsync(item.Id);

        Assert.Equal(0, await _context.ToDoItems.CountAsync(ct));
    }

    // PatchAsync

    [Fact]
    public async Task PatchAsync_DoesNothing_WhenItemDoesNotExist()
    {
        var ct = TestContext.Current.CancellationToken;
        var patchDoc = new JsonPatchDocument<ToDoItem>();
        patchDoc.Replace(t => t.Title, "Patched");
        await _service.PatchAsync(999, patchDoc);
        Assert.Equal(0, await _context.ToDoItems.CountAsync(ct));
    }

    [Fact]
    public async Task PatchAsync_AppliesTitle_WhenItemExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new ToDoItem { Title = "Original", IsComplete = false };
        _context.ToDoItems.Add(item);
        await _context.SaveChangesAsync(ct);

        var patchDoc = new JsonPatchDocument<ToDoItem>();
        patchDoc.Replace(t => t.Title, "Patched");

        await _service.PatchAsync(item.Id, patchDoc);

        var fromDb = await _context.ToDoItems.FindAsync([item.Id], ct);
        Assert.Equal("Patched", fromDb!.Title);
    }

    [Fact]
    public async Task PatchAsync_AppliesIsComplete_WhenItemExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new ToDoItem { Title = "Task", IsComplete = false };
        _context.ToDoItems.Add(item);
        await _context.SaveChangesAsync(ct);
        var patchDoc = new JsonPatchDocument<ToDoItem>();
        patchDoc.Replace(t => t.IsComplete, true);
        await _service.PatchAsync(item.Id, patchDoc);
        var fromDb = await _context.ToDoItems.FindAsync([item.Id], ct);
        Assert.True(fromDb!.IsComplete);
    }
}
