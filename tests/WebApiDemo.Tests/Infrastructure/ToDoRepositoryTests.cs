using Microsoft.EntityFrameworkCore;
using WebApiDemo.Domain.Entities;
using WebApiDemo.Infrastructure.Persistence;
using WebApiDemo.Infrastructure.Repositories;
using Xunit;

namespace WebApiDemo.Tests.Infrastructure;

public class ToDoRepositoryTests : IDisposable
{
    private readonly ToDoDbContext _context;
    private readonly ToDoRepository _repository;

    public ToDoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ToDoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ToDoDbContext(options);
        _repository = new ToDoRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    // GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoItemsExist()
    {
        var result = await _repository.GetAllAsync();
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

        var result = await _repository.GetAllAsync();

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

        var result = await _repository.GetByIdAsync(item.Id);

        Assert.NotNull(result);
        Assert.Equal("Find me", result.Title);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        var result = await _repository.GetByIdAsync(999);
        Assert.Null(result);
    }

    // AddAsync

    [Fact]
    public async Task AddAsync_ReturnsAddedItem_WithGeneratedId()
    {
        var item = new ToDoItem { Title = "New Task", IsComplete = false };

        var result = await _repository.AddAsync(item);

        Assert.True(result.Id > 0);
        Assert.Equal("New Task", result.Title);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task AddAsync_PersistsItem_InDatabase()
    {
        var ct = TestContext.Current.CancellationToken;
        await _repository.AddAsync(new ToDoItem { Title = "Persisted", IsComplete = true });

        Assert.Equal(1, await _context.ToDoItems.CountAsync(ct));
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        var result = await _repository.UpdateAsync(999, new ToDoItem { Title = "Updated", IsComplete = true });
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedItem_WhenItemExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new ToDoItem { Title = "Original", IsComplete = false };
        _context.ToDoItems.Add(item);
        await _context.SaveChangesAsync(ct);

        var result = await _repository.UpdateAsync(item.Id, new ToDoItem { Title = "Updated", IsComplete = true });

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

        await _repository.UpdateAsync(item.Id, new ToDoItem { Title = "Persisted", IsComplete = true });

        var fromDb = await _context.ToDoItems.FindAsync([item.Id], ct);
        Assert.Equal("Persisted", fromDb!.Title);
        Assert.True(fromDb.IsComplete);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        var result = await _repository.DeleteAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsDeletedItem_WhenItemExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var item = new ToDoItem { Title = "To Delete", IsComplete = false };
        _context.ToDoItems.Add(item);
        await _context.SaveChangesAsync(ct);

        var result = await _repository.DeleteAsync(item.Id);

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

        await _repository.DeleteAsync(item.Id);

        Assert.Equal(0, await _context.ToDoItems.CountAsync(ct));
    }
}
