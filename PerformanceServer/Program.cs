// Copyright (c) 2025 GooGuTeam
// Licensed under the MIT Licence. See the LICENCE file in the repository root for full licence text.

namespace PerformanceServer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            WebApplication app = builder.Build();

            // Simple root endpoint for quick health / smoke checks.
            app.MapGet("/", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

            app.MapControllers();
            app.Run();
        }
    }
}