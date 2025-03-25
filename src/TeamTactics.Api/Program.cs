using TeamTactics.Api.Middleware;
using TeamTactics.Application;
using TeamTactics.Infrastructure;
using Serilog;
using DbMigrator;
using TeamTactics.Infrastructure.Tokens;
using TeamTactics.Api.Configurations;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
//try
//{
var builder = WebApplication.CreateBuilder(args);

    string? connString = builder.Configuration.GetConnectionString("Postgres");
var conf = builder.Configuration;

    if (string.IsNullOrWhiteSpace(connString))
    {
        throw new InvalidOperationException("Connection string not found");
    }

    if (builder.Environment.IsDevelopment())
        if (DatabaseMigrator.MigrateDatabase(connString, true) == 0)
            DatabaseMigrator.MigrateDatabase(connString);

    // Serilog to ASPNET
    builder.Services.AddSerilog(options =>
    {
        options.ReadFrom.Configuration(builder.Configuration);
    });

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(connString);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.SetupSwagger();

    builder.Services.AddExceptionHandler<GlobalExceptionHandling>();
    builder.Services.AddProblemDetails();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.SetupOptions();

    builder.Services.SetupJwt(builder.Configuration.GetRequiredSection("JWT").Get<JwtOptions>()
        ?? throw new InvalidOperationException("Unable to get Jwt options."));

    builder.Services.SetupCors();

    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });

    var app = builder.Build();

    app.UseCors(CorsSetup.CORS_POLICY);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/V1/swagger.json", "TeamTactics API");
        });
    }

    app.MapHealthChecks("/api/health");

    app.UseExceptionHandler();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
//}
//catch (Exception ex)
//{
//    Log.Fatal(ex, "Application terminated unexpectedly");
//    if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
//        throw;
//}
//finally
//{
//    Log.CloseAndFlush();
//}

public partial class Program;