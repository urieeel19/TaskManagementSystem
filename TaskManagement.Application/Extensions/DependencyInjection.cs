using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaskManagement.Application.Interfaces.Common;

namespace TaskManagement.Application.Extensions
{
    /// <summary>
    /// Provides extension methods to configure Application layer services in the dependency injection container.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers all Application layer core behaviors, including automatic CQS handlers scan and FluentValidation rules.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> data structure context to add services to.</param>
        /// <returns>The same service collection contract definition for chaining pattern invocations.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Retrieve the current assembly where the Application layer use cases reside
            var assembly = Assembly.GetExecutingAssembly();

            // 1. GENERIC AND AUTOMATIC USE CASE REGISTRATION (CQS HANDLERS)
            // Scan and inject all Command Handlers that return a concrete response payload (e.g., Result<CreateTaskResponse>)
            services.ScanAssemblyHandlers(assembly, typeof(ICommandHandler<,>));

            // Scan and inject all Query Handlers optimized for read operations (e.g., GetById, GetTasksByUserId)
            services.ScanAssemblyHandlers(assembly, typeof(IQueryHandler<,>));


            // 2. AUTOMATIC VALIDATION MECHANISM CONFIGURATION (FLUENTVALIDATION)
            // Searches for any implementation matching AbstractValidator within this assembly and sets up cross-cutting validation pipelines
            services.AddValidatorsFromAssembly(assembly);


            return services;
        }

        /// <summary>
        /// Private helper utility that performs metadata reflection scanning over the target assembly matching specific interface blueprints.
        /// </summary>
        /// <param name="services">The application dependency injection container pipeline.</param>
        /// <param name="assembly">The isolated assembly payload containing the application types.</param>
        /// <param name="interfaceType">The generic type abstraction definition acting as the target search criteria filter.</param>
        private static void ScanAssemblyHandlers(this IServiceCollection services, Assembly assembly, Type interfaceType)
        {
            var handlers = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .SelectMany(t => t.GetInterfaces(), (types, interfaces) => new { types, interfaces })
                .Where(x => x.interfaces.IsGenericType && x.interfaces.GetGenericTypeDefinition() == interfaceType);

            foreach (var handler in handlers)
            {
                services.AddScoped(handler.interfaces, handler.types);
            }
        }
    }
}
