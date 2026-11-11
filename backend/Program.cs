using backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// DbContext PostgreSQL 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    var retries = 10;
    while (retries > 0)
    {
        try
        {
            logger.LogInformation("Applying database migrations...");
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning(ex, "Database connection not ready yet. Retrying in 2 seconds... ({RetriesLeft} attempts left)", retries);
            if (retries == 0)
            {
                logger.LogError(ex, "Failed to connect to database and apply migrations.");
                throw;
            }
            Thread.Sleep(2000);
        }
    }
}

app.Run();
