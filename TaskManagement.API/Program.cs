using FluentValidation;
using TaskManagement.API.Extensions; 
using TaskManagement.API.Middleware;
using TaskManagement.Application.UseCases.Auth.Login;
using TaskManagement.Infrastructure.DependencyInjection; 
using TaskManagement.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicies();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddValidatorsFromAssembly(typeof(LoginCommand).Assembly);

var app = builder.Build();

await app.SeedDatabaseAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AngularClientPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
