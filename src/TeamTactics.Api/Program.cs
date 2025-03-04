using TeamTactics.Api.Middleware;
using TeamTactics.Application;
using TeamTactics.Application.Common.Options;
using TeamTactics.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
// Health Checks
var conString = builder.Configuration.GetConnectionString("Postgres");
if (string.IsNullOrEmpty(conString))
{
    throw new InvalidOperationException("Connection string not found");
}
builder.Services.AddHealthChecks()
    .AddNpgSql(conString);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandling>();
builder.Services.AddProblemDetails();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
// Options
builder.Services.AddOptions<PasswordSecurityOptions>()
    .Bind(builder.Configuration.GetSection("PasswordSecurity"))
    .ValidateDataAnnotations();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHealthChecks("/health");
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;