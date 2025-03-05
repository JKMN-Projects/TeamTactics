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

    string? connString = builder.Configuration.GetConnectionString("Postgres");

    if (string.IsNullOrWhiteSpace(connString))
    {
        throw new InvalidOperationException("Connection string not found");
    }

    if (builder.Environment.IsDevelopment())
        if (DatabaseMigrator.MigrateDatabase(connString, true) == 0)
            DatabaseMigrator.MigrateDatabase(connString);

    // Serilog to ASPNET
    builder.Services.AddSerilog();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(connString);

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddExceptionHandler<GlobalExceptionHandling>();
    builder.Services.AddProblemDetails();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

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

    app.MapHealthChecks("/api/health");

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