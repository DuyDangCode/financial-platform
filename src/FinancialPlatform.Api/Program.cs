using FinancialPlatform.Application;
using FinancialPlatform.Infrastructure;
using FinancialPlatform.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<FinancialPlatform.Api.Middleware.GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

await FinancialPlatform.Infrastructure.Persistence.SeedData.SeedAsync(app.Services);

app.Run();
