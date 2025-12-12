using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Api.Controllers;
using TodoApp.Api.Data;
using TodoApp.Api.Models;
using Xunit;

namespace TodoApp.Tests.Controllers
{
    public class TodosControllerTests
    {
        private DbContextOptions<AppDbContext> _dbContextOptions;

        public TodosControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetTodos_ReturnsOnlyActiveTodos()
        {
            using (var context = new AppDbContext(_dbContextOptions))
            {
                context.TodoItems.Add(new TodoItem { Title = "Active 1", IsDone = false });
                context.TodoItems.Add(new TodoItem { Title = "Done 1", IsDone = true });
                context.TodoItems.Add(new TodoItem { Title = "Active 2", IsDone = false });
                await context.SaveChangesAsync();
            }

            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new TodosController(context);
                var result = await controller.GetTodos();

                var actionResult = Assert.IsType<ActionResult<IEnumerable<TodoItem>>>(result);
                var items = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(actionResult.Value);

                Assert.Equal(2, items.Count());
                Assert.DoesNotContain(items, t => t.IsDone);
            }
        }

        [Fact]
        public async Task GetHistory_ReturnsOnlyDoneTodos()
        {
            using (var context = new AppDbContext(_dbContextOptions))
            {
                context.TodoItems.Add(new TodoItem { Title = "Active 1", IsDone = false });
                context.TodoItems.Add(new TodoItem { Title = "Done 1", IsDone = true });
                await context.SaveChangesAsync();
            }

            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new TodosController(context);
                var result = await controller.GetHistory();

                var actionResult = Assert.IsType<ActionResult<IEnumerable<TodoItem>>>(result);
                var items = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(actionResult.Value);

                Assert.Single(items);
                Assert.Contains(items, t => t.IsDone);
            }
        }

        [Fact]
        public async Task PostTodoItem_CreatesItem()
        {
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new TodosController(context);
                var newItem = new TodoItem { Title = "New Item" };

                var result = await controller.PostTodoItem(newItem);

                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                var item = Assert.IsType<TodoItem>(createdAtActionResult.Value);
                Assert.Equal("New Item", item.Title);
                Assert.NotEqual(Guid.Empty, item.Id);
                Assert.False(item.IsDone);
            }
        }

        [Fact]
        public async Task PutTodoItem_UpdatesItemAndMetadata()
        {
            Guid itemId;
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var item = new TodoItem { Title = "Original", Weekday = DayOfWeek.Monday, MovedCounter = 0 };
                context.TodoItems.Add(item);
                await context.SaveChangesAsync();
                itemId = item.Id;
            }

            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new TodosController(context);
                var itemToUpdate = new TodoItem
                {
                    Id = itemId,
                    Title = "Updated",
                    Weekday = DayOfWeek.Tuesday, // Changed
                    IsDone = true // Changed
                };

                var result = await controller.PutTodoItem(itemId, itemToUpdate);

                Assert.IsType<NoContentResult>(result);

                var updatedItem = await context.TodoItems.FindAsync(itemId);
                Assert.Equal("Updated", updatedItem?.Title);
                Assert.Equal(DayOfWeek.Tuesday, updatedItem?.Weekday);
                Assert.Equal(1, updatedItem?.MovedCounter); // Should increment
                Assert.True(updatedItem?.IsDone);
                Assert.NotNull(updatedItem?.ResolvedAt); // Should be set
            }
        }
    }
}
