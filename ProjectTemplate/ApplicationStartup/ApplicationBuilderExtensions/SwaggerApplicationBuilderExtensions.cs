using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ProjectTemplate.Constants;
using ProjectTemplate.Middleware;

namespace ProjectTemplate.ApplicationStartup.ApplicationBuilderExtensions
{
    public static class SwaggerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseAndConfigureSwagger(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<SwaggerBasicAuthMiddleware>();
            app.UseSwagger();

            if (env.IsDevelopment())
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(Settings.SwaggerDocV1Endpoint, FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName);
                });
            }
            else
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"../{Settings.SwaggerDocV1Endpoint}", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName);
                });
            }

            return app;
        }
    }
}