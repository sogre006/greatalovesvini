using PeriodTracker.Model.Repositories;

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
               .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowAngularApp");

// Use authorization middleware
app.UseAuthorization();

app.MapControllers();

app.Run();