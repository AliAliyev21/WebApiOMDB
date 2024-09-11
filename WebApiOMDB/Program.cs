using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApiOMDB.Data;
using WebApiOMDB.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<OmdbDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the background service
builder.Services.AddHostedService<MovieBackgroundWorker>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Use CORS
app.UseCors("AllowAll");

// Use health checks
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
