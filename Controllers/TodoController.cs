using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using webapi_demo.DTO;
using webapi_demo.Interfaces;
using webapi_demo.Models;

namespace webapi_demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly IToDoService _toDoService;

        public TodoController(IToDoService toDoService)
        {
            _toDoService = toDoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _toDoService.GetAllAsync();
            if (!items.Any()) return NoContent();
            var itemDTO = items.Select(MapToToDoItemDTO).ToList();
            return Ok(itemDTO);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetToDoItemById(int id)
        {
            var item = await _toDoService.GetByIdAsync(id);
            return (item == null) ? NotFound() : Ok(MapToToDoItemDTO(item));
        }

        [HttpPost]
        public async Task<IActionResult> CreateToDoItem([FromBody] ToDoItemDTO newItemDto)
        {
            if (newItemDto == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var item = new ToDoItem()
            {
                Title = newItemDto.Title,
                IsComplete = newItemDto.IsComplete
            };

            item = await _toDoService.AddAsync(item);
            var itemDto = MapToToDoItemDTO(item);
            return CreatedAtAction(nameof(GetToDoItemById), new {id = item.Id}, itemDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateToDoItem(int id, [FromBody] ToDoItemDTO updateItemDto)
        {

            if (updateItemDto == null) return BadRequest();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var item = new ToDoItem()
            {
                Title = updateItemDto.Title,
                IsComplete = updateItemDto.IsComplete
            };

            item = await _toDoService.UpdateAsync(id, item);
            return (item == null) ? NotFound() : Ok(MapToToDoItemDTO(item));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchToDoItem(int id, [FromBody] JsonPatchDocument<ToDoItem> patchDoc)
        {
            await _toDoService.PatchAsync(id, patchDoc);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToDoItem(int id)
        {
            var item = await _toDoService.DeleteAsync(id);
            return (item == null) ? NotFound() : Ok(MapToToDoItemDTO(item));
        }

        private ToDoItemDTO MapToToDoItemDTO(ToDoItem item)
        {
            return new ToDoItemDTO
            {
                Id = item.Id,
                Title = item.Title,
                IsComplete = item.IsComplete,
            };
        }
    }
}
