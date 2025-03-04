using TeamTactics.Api.Middleware;
using TeamTactics.Application;
using TeamTactics.Application.Common.Options;
using TeamTactics.Infrastructure;
using Serilog;
using DbMigrator;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    string? conString = builder.Configuration.GetConnectionString("Postgres");

    if (string.IsNullOrWhiteSpace(conString))
    {
        throw new InvalidOperationException("Connection string not found");
    }

    if (DatabaseMigrator.MigrateDatabase(conString, true) == 0)
        DatabaseMigrator.MigrateDatabase(conString);

    // Serilog to ASPNET
    builder.Services.AddSerilog();

    // Health Checks
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
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;