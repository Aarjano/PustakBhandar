using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace FinalProject.Infrastructure
{
    public class OptionsRequestStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseRouting();
                
                // Add middleware to handle OPTIONS requests before anything else
                app.Use(async (context, nextMiddleware) =>
                {
                    if (context.Request.Method == "OPTIONS")
                    {
                        // Set CORS headers directly
                        context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
                        context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization");
                        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                        
                        // Return 200 OK immediately for OPTIONS requests
                        context.Response.StatusCode = 200;
                        await context.Response.CompleteAsync();
                        return;
                    }
                    
                    await nextMiddleware();
                });
                
                next(app);
            };
        }
    }
} 