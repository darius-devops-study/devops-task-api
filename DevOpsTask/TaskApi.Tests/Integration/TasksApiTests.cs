using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskApi.Models;
using Xunit;

namespace TaskApi.Tests.Integration
{
    public class TasksApiTests
    {
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CreateAndComplete_TaskIsSavedAndCompleted()
        {
            using (WebApplicationFactory<TaskApi.Program> application =
                   new WebApplicationFactory<TaskApi.Program>())
            using (HttpClient client = application.CreateClient())
            {
                CreateTaskRequest request = new CreateTaskRequest();
                request.Title = "Проверить API";

                HttpResponseMessage createResponse =
                    await client.PostAsJsonAsync("/api/tasks", request);

                Assert.Equal(
                    HttpStatusCode.Created,
                    createResponse.StatusCode);

                TaskItem? createdTask =
                    await createResponse.Content.ReadFromJsonAsync<TaskItem>();

                Assert.NotNull(createdTask);
                Assert.Equal("Проверить API", createdTask.Title);
                Assert.False(createdTask.IsCompleted);

                HttpResponseMessage completeResponse =
                    await client.PutAsync(
                        "/api/tasks/" + createdTask.Id + "/complete",
                        null);

                Assert.Equal(
                    HttpStatusCode.NoContent,
                    completeResponse.StatusCode);

                HttpResponseMessage getResponse =
                    await client.GetAsync("/api/tasks/" + createdTask.Id);

                Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

                TaskItem? savedTask =
                    await getResponse.Content.ReadFromJsonAsync<TaskItem>();

                Assert.NotNull(savedTask);
                Assert.Equal(createdTask.Id, savedTask.Id);
                Assert.True(savedTask.IsCompleted);
            }
        }

        [Theory]
        [Trait("Category", "Integration")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Create_ReturnsBadRequestForEmptyTitle(string title)
        {
            using (WebApplicationFactory<TaskApi.Program> application =
                   new WebApplicationFactory<TaskApi.Program>())
            using (HttpClient client = application.CreateClient())
            {
                CreateTaskRequest request = new CreateTaskRequest();
                request.Title = title;

                HttpResponseMessage response =
                    await client.PostAsJsonAsync("/api/tasks", request);

                Assert.Equal(
                    HttpStatusCode.BadRequest,
                    response.StatusCode);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetById_ReturnsNotFoundForMissingTask()
        {
            using (WebApplicationFactory<TaskApi.Program> application =
                   new WebApplicationFactory<TaskApi.Program>())
            using (HttpClient client = application.CreateClient())
            {
                HttpResponseMessage response =
                    await client.GetAsync("/api/tasks/999");

                Assert.Equal(
                    HttpStatusCode.NotFound,
                    response.StatusCode);
            }
        }
    }
}