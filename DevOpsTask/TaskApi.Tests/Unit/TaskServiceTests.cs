using TaskApi.Models;
using TaskApi.Services;
using Xunit;

namespace TaskApi.Tests.Unit
{
    public class TaskServiceTests
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void Create_AddsTaskToList()
        {
            TaskService service = new TaskService();

            TaskItem task = service.Create("Настроить Docker");

            Assert.Equal(1, task.Id);
            Assert.Equal("Настроить Docker", task.Title);
            Assert.False(task.IsCompleted);
            Assert.Single(service.GetAll());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Create_AssignsDifferentIds()
        {
            TaskService service = new TaskService();

            TaskItem firstTask = service.Create("Первая задача");
            TaskItem secondTask = service.Create("Вторая задача");

            Assert.NotEqual(firstTask.Id, secondTask.Id);
            Assert.Equal(2, service.GetAll().Count);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Complete_ChangesTaskStatus()
        {
            TaskService service = new TaskService();
            TaskItem task = service.Create("Написать тесты");

            bool result = service.Complete(task.Id);

            Assert.True(result);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Complete_ReturnsFalseForMissingTask()
        {
            TaskService service = new TaskService();

            bool result = service.Complete(999);

            Assert.False(result);
            Assert.Empty(service.GetAll());
        }
    }
}