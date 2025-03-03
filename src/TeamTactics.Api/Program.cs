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

    try
    {
        string? connectionString = builder.Configuration["LocalConnectionString"];

        if (connectionString != null)
            if (DbMigrator.DatabaseMigrator.MigrateDatabase(connectionString, true) == 0)
                DbMigrator.DatabaseMigrator.MigrateDatabase(connectionString);
    }
    catch { }

    // Add services to the container.
    builder.Services.AddSerilog();

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