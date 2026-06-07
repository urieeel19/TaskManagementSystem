using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Extensions
{
    public static class AppExtensions
    {
        /// <summary>
        /// Seeds the MongoDB database container safely during application startup.
        /// </summary>
        public static async Task SeedDatabaseAsync(this IHost app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var initializer = services.GetRequiredService<DbInitializer>();
                await initializer.SeedAsync();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<DbInitializer>>();
                logger.LogError(ex, "An error occurred while running execution seeding on MongoDB container.");
            }
        }
    }
}
