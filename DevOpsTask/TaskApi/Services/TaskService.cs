using System.Collections.Generic;
using TaskApi.Models;

namespace TaskApi.Services
{
    public class TaskService
    {
        private readonly List<TaskItem> tasks = new List<TaskItem>();
        private int nextId = 1;

        public List<TaskItem> GetAll()
        {
            return tasks;
        }

        public TaskItem? GetById(int id)
        {
            foreach (TaskItem task in tasks)
            {
                if (task.Id == id)
                {
                    return task;
                }
            }

            return null;
        }

        public TaskItem Create(string title)
        {
            TaskItem task = new TaskItem();

            task.Id = nextId;
            task.Title = title;
            task.IsCompleted = false;

            tasks.Add(task);
            nextId = nextId + 1;

            return task;
        }

        public bool Complete(int id)
        {
            TaskItem? task = GetById(id);

            if (task == null)
            {
                return false;
            }

            task.IsCompleted = true;

            return true;
        }
    }
}