using Microsoft.AspNetCore.Mvc;
using TaskApi.Models;
using TaskApi.Services;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService taskService;

        public TasksController(TaskService taskService)
        {
            this.taskService = taskService;
        }

        [HttpGet]
        public ActionResult<List<TaskItem>> GetAll()
        {
            List<TaskItem> tasks = taskService.GetAll();

            return Ok(tasks);
        }

        [HttpGet("{id:int}")]
        public ActionResult<TaskItem> GetById(int id)
        {
            TaskItem? task = taskService.GetById(id);

            if (task == null)
            {
                return NotFound("Задача не найдена.");
            }

            return Ok(task);
        }

        [HttpPost]
        public ActionResult<TaskItem> Create(CreateTaskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Название задачи не должно быть пустым.");
            }

            TaskItem task = taskService.Create(request.Title.Trim());

            return CreatedAtAction(
                nameof(GetById),
                new { id = task.Id },
                task);
        }

        [HttpPut("{id:int}/complete")]
        public IActionResult Complete(int id)
        {
            bool completed = taskService.Complete(id);

            if (completed == false)
            {
                return NotFound("Задача не найдена.");
            }

            return NoContent();
        }
    }
}