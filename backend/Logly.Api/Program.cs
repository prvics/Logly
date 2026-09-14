using Logly.Infrastructure;
using Logly.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<LoglyDbContext>("database");

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Ok(new
{
    name = "Logly API",
    status = "Running"
}));

app.MapHealthChecks("/health");

app.UseAuthorization();

app.MapControllers();

app.Run();