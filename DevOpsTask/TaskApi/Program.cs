using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TaskApi.Services;

namespace TaskApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder =
                WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddSingleton<TaskService>();

            WebApplication app = builder.Build();

            app.MapControllers();

            app.Run();
        }
    }
}