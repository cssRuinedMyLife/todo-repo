using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;
using TodoApp.Api.Models;

namespace TodoApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TodosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/todos
        // Returns all active (not done) todos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos([FromQuery] DayOfWeek? weekday = null)
        {
            var query = _context.TodoItems.Where(t => !t.IsDone);

            if (weekday.HasValue)
            {
                query = query.Where(t => t.Weekday == weekday.Value);
            }

            return await query.OrderBy(t => t.OrderIndex).ThenBy(t => t.CreatedAt).ToListAsync();
        }

        // GET: api/todos/history
        // Returns all done todos
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetHistory()
        {
            return await _context.TodoItems
                .Where(t => t.IsDone)
                .OrderByDescending(t => t.ResolvedAt)
                .ToListAsync();
        }

        // GET: api/todos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(Guid id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // POST: api/todos
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        {
            todoItem.Id = Guid.NewGuid();
            todoItem.CreatedAt = DateTime.UtcNow;
            todoItem.IsDone = false;
            todoItem.ResolvedAt = null;
            todoItem.MovedCounter = 0;

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        // PUT: api/todos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(Guid id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            var existingItem = await _context.TodoItems.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            // Update fields
            existingItem.Title = todoItem.Title;
            existingItem.Description = todoItem.Description;
            existingItem.Category = todoItem.Category;
            existingItem.OrderIndex = todoItem.OrderIndex;
            
            // Handle Weekday change (Moved logic)
            if (existingItem.Weekday != todoItem.Weekday)
            {
                existingItem.Weekday = todoItem.Weekday;
                existingItem.MovedCounter++;
            }

            // Handle IsDone change (Resolved logic)
            if (todoItem.IsDone && !existingItem.IsDone)
            {
                existingItem.IsDone = true;
                existingItem.ResolvedAt = DateTime.UtcNow;
            }
            else if (!todoItem.IsDone && existingItem.IsDone)
            {
                existingItem.IsDone = false;
                existingItem.ResolvedAt = null;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/todos/5
        // Optional: Hard delete if really needed, but user said "not deleted". 
        // I'll leave it out or implement it as hard delete for cleanup purposes if requested.
        // For now, omitting to strictly follow "A done item ... is not deleted".
        // But usually a way to really delete (e.g. mistake) is good. 
        // I'll add it but maybe comment it out or keep it for "admin" purposes? 
        // The user said "A done item is not visible anymore, but is not deleted". This refers to the "Done" action.
        // It doesn't explicitly forbid a "Delete" action for mistakes.
        // "Todo items should be editable with CRUD functions." -> CRUD includes Delete.
        // So I WILL implement Delete, but maybe it's a hard delete.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(Guid id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoItemExists(Guid id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}
