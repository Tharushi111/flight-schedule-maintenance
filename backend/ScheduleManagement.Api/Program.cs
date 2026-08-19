using ScheduleManagement.Api.Repositories;
using ScheduleManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevelopmentCors =
    "AngularDevelopment";

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Dependency Injection

builder.Services.AddScoped<
    IAirportRepository,
    AirportRepository>();

builder.Services.AddScoped<
    IScheduleRepository,
    ScheduleRepository>();

builder.Services.AddScoped<
    IScheduleService,
    ScheduleService>();


// CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        AngularDevelopmentCors,
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


var app = builder.Build();


// Development tools

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



// HTTP Pipeline

app.UseHttpsRedirection();

app.UseCors(AngularDevelopmentCors);

app.UseAuthorization();

app.MapControllers();

app.Run();