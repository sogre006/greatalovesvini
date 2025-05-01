using PeriodTracker.API.Middleware;
using PeriodTracker.Model.Repositories;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<CalendarRepository>();
builder.Services.AddScoped<PeriodCycleRepository>();
builder.Services.AddScoped<CycleEntryRepository>();

// Configure CORS to allow requests from Angular application
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", builder =>
    {
        builder.WithOrigins("http://localhost:4200") // Angular app's default URL
               .AllowAnyHeader()
               .AllowAnyMethod()
               .WithExposedHeaders("Authorization");
    });
});

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    // Default policy
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add exception handling middleware
app.UseExceptionHandler("/error");

// Enable CORS
app.UseCors("AllowAngularApp");

// Add Basic Authentication middleware before Authorization
app.UseBasicAuthenticationMiddleware();

// Use authorization middleware
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Start the application
app.Run();