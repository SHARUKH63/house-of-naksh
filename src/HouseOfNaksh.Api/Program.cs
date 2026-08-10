using HouseOfNaksh.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

app.MapHealthChecks("/health");

app.MapGet("/config-check", (IConfiguration cfg) => new {
    hasRazorpayKey = !string.IsNullOrEmpty(cfg["Payments:RazorpayKeyId"]),
    prefix = cfg["Payments:RazorpayKeyId"]?[..Math.Min(8, cfg["Payments:RazorpayKeyId"]!.Length)]
});

app.UseAuthorization();

app.MapControllers();

app.Run();
