using HouseOfNaksh.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();

builder.Host.UseSerilog((context, services, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "HouseOfNaksh.Api")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.ApplicationInsights(
        services.GetRequiredService<TelemetryConfiguration>(),
        new TraceTelemetryConverter()));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<HouseOfNakshDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("HouseOfNakshDb"),
        sql =>
        {
            sql.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sql.CommandTimeout(60);
        }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.GetLevel = (httpContext, elapsed, ex) =>
        ex != null || httpContext.Response.StatusCode > 499
            ? LogEventLevel.Error
            : httpContext.Request.Path.StartsWithSegments("/health")
                ? LogEventLevel.Verbose
                : LogEventLevel.Information;

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("TraceId", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty);
    };
});

app.MapHealthChecks("/health");

app.MapGet("/config-check", (IConfiguration cfg) => new {
    hasRazorpayKey = !string.IsNullOrEmpty(cfg["Payments:RazorpayKeyId"]),
    prefix = cfg["Payments:RazorpayKeyId"]?[..Math.Min(8, cfg["Payments:RazorpayKeyId"]!.Length)]
});

app.UseAuthorization();

app.MapControllers();

app.Run();