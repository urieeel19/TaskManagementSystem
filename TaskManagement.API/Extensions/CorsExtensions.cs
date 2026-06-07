namespace TaskManagement.API.Extensions
{
    public static class CorsExtensions
    {
        private const string AngularPolicyName = "AngularClientPolicy";

        public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(AngularPolicyName, policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
