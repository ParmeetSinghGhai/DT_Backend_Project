using APIService.Components;
using APIService.Models;
using System.Text.Json;
using System.Diagnostics;

Database.Connect();

var builder = WebApplication.CreateBuilder();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORSPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:5000")
                                .AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});
builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();


