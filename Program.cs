using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using Scalar.AspNetCore;
using TmsApi.Entities;
using TmsApi.Filters;
using TmsApi.Services;
using TmsApi.Persistence;
using TmsApi.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
.LogTo(Console.WriteLine, LogLevel.Information).EnableSensitiveDataLogging());

builder.Services.AddAuthorization();


builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

//  Session 2: Service registrations 
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();


builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

var app = builder.Build();


//  Session 1: Logging middleware — must be outermost 
app.UseMiddleware<RequestLoggingMiddleware>();




    app.MapOpenApi();
    app.MapScalarApiReference();
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
        await DataSeeder.SeedAsync(context);
    }




app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();




app.Run();