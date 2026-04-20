using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebApiDemo.Application.DTOs;
using WebApiDemo.Application.Interfaces;
using WebApiDemo.Domain.Entities;
using WebApiDemo.WebAPI.Controllers;
using Xunit;

namespace WebApiDemo.Tests.WebAPI;

public class TodoControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsNoContent_WhenServiceReturnsNoItems()
    {
        var service = new FakeToDoService
        {
            GetAllResult = Array.Empty<ToDoItem>()
        };
        var controller = new TodoController(service);

        var result = await controller.GetAll();

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithMappedDtos_WhenItemsExist()
    {
        var service = new FakeToDoService
        {
            GetAllResult =
            [
                new ToDoItem { Id = 1, Title = "First", IsComplete = false },
                new ToDoItem { Id = 2, Title = "Second", IsComplete = true }
            ]
        };
        var controller = new TodoController(service);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<ToDoItemDTO>>(okResult.Value);
        var mappedItems = items.ToList();

        Assert.Equal(2, mappedItems.Count);
        Assert.Equal("First", mappedItems[0].Title);
        Assert.True(mappedItems[1].IsComplete);
    }

    [Fact]
    public async Task GetToDoItemById_ReturnsOk_WhenItemExists()
    {
        var service = new FakeToDoService
        {
            GetByIdResult = new ToDoItem { Id = 1, Title = "Found", IsComplete = true }
        };
        var controller = new TodoController(service);

        var result = await controller.GetToDoItemById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<ToDoItemDTO>(okResult.Value);
        Assert.Equal(1, dto.Id);
        Assert.Equal("Found", dto.Title);
        Assert.True(dto.IsComplete);
    }

    [Fact]
    public async Task GetToDoItemById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        var controller = new TodoController(new FakeToDoService());

        var result = await controller.GetToDoItemById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateToDoItem_ReturnsCreatedAtAction_WithMappedDto()
    {
        var service = new FakeToDoService
        {
            AddResult = new ToDoItem { Id = 7, Title = "Created", IsComplete = true }
        };
        var controller = new TodoController(service);
        var dto = new ToDoItemDTO { Title = "Created", IsComplete = true };

        var result = await controller.CreateToDoItem(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(TodoController.GetToDoItemById), createdResult.ActionName);

        var returnedDto = Assert.IsType<ToDoItemDTO>(createdResult.Value);
        Assert.Equal(7, returnedDto.Id);
        Assert.Equal("Created", returnedDto.Title);
        Assert.True(returnedDto.IsComplete);
    }

    [Fact]
    public async Task CreateToDoItem_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var controller = new TodoController(new FakeToDoService());
        controller.ModelState.AddModelError("Title", "Title is required");

        var result = await controller.CreateToDoItem(new ToDoItemDTO());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateToDoItem_ReturnsOk_WhenItemExists()
    {
        var service = new FakeToDoService
        {
            UpdateResult = new ToDoItem { Id = 5, Title = "Updated", IsComplete = true }
        };
        var controller = new TodoController(service);
        var dto = new ToDoItemDTO { Title = "Updated", IsComplete = true };

        var result = await controller.UpdateToDoItem(5, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<ToDoItemDTO>(okResult.Value);
        Assert.Equal(5, returnedDto.Id);
        Assert.Equal("Updated", returnedDto.Title);
    }

    [Fact]
    public async Task UpdateToDoItem_ReturnsNotFound_WhenServiceReturnsNull()
    {
        var controller = new TodoController(new FakeToDoService());
        var dto = new ToDoItemDTO { Title = "Updated", IsComplete = false };

        var result = await controller.UpdateToDoItem(15, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateToDoItem_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var controller = new TodoController(new FakeToDoService());
        controller.ModelState.AddModelError("Title", "Title is required");

        var result = await controller.UpdateToDoItem(1, new ToDoItemDTO());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteToDoItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        var controller = new TodoController(new FakeToDoService());

        var result = await controller.DeleteToDoItem(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteToDoItem_ReturnsOk_WhenItemIsDeleted()
    {
        var service = new FakeToDoService
        {
            DeleteResult = new ToDoItem { Id = 3, Title = "Deleted", IsComplete = false }
        };
        var controller = new TodoController(service);

        var result = await controller.DeleteToDoItem(3);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<ToDoItemDTO>(okResult.Value);
        Assert.Equal(3, returnedDto.Id);
        Assert.Equal("Deleted", returnedDto.Title);
    }

    [Fact]
    public async Task PatchToDoItem_ReturnsNoContent_AndInvokesService()
    {
        var service = new FakeToDoService();
        var controller = new TodoController(service);
        var patchDoc = new JsonPatchDocument<ToDoItem>();

        var result = await controller.PatchToDoItem(4, patchDoc);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(4, service.PatchCalledWithId);
        Assert.Same(patchDoc, service.PatchCalledWithDocument);
    }

    private sealed class FakeToDoService : IToDoService
    {
        public IEnumerable<ToDoItem> GetAllResult { get; init; } = Array.Empty<ToDoItem>();
        public ToDoItem? GetByIdResult { get; init; }
        public ToDoItem AddResult { get; init; } = new();
        public ToDoItem? UpdateResult { get; init; }
        public ToDoItem? DeleteResult { get; init; }
        public int? PatchCalledWithId { get; private set; }
        public JsonPatchDocument<ToDoItem>? PatchCalledWithDocument { get; private set; }

        public Task<IEnumerable<ToDoItem>> GetAllAsync() => Task.FromResult(GetAllResult);
        public Task<ToDoItem?> GetByIdAsync(int id) => Task.FromResult(GetByIdResult);
        public Task<ToDoItem> AddAsync(ToDoItem newItem) => Task.FromResult(AddResult);
        public Task<ToDoItem?> UpdateAsync(int id, ToDoItem updatedItem) => Task.FromResult(UpdateResult);
        public Task<ToDoItem?> DeleteAsync(int id) => Task.FromResult(DeleteResult);

        public Task PatchAsync(int id, JsonPatchDocument<ToDoItem> patchDoc)
        {
            PatchCalledWithId = id;
            PatchCalledWithDocument = patchDoc;
            return Task.CompletedTask;
        }
    }
}
