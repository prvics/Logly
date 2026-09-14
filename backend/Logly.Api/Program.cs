var builder = WebApplication.CreateBuilder(args);

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

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy"
}));

app.UseAuthorization();

app.MapControllers();

app.Run();