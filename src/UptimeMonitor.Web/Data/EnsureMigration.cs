using Microsoft.EntityFrameworkCore;

namespace UptimeMonitor.Web.Data
{
    public static class EnsureMigration
    {
        public static void EnsureMigrationOfContext<T>(this IApplicationBuilder app) where T : DbContext
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<T>();

                if (context != null)
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}
