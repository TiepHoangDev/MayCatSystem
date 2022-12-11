using Blazored.SessionStorage;
using MayCatSystem.WebUI.Data;
using MayCatSystem.WebUI.Models;
using MayCatSystem.WebUI.Services;
using MayCatSystem.WebUI.Services.BackgroundServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<AuthenticationStateProvider, WebAuthenticationStateProvider>();
builder.Services.AddScoped<AccountService>();

//DataKepserverService
builder.Services.AddSingleton<DataKepserverService>();
builder.Services.AddHostedService(p => p.GetRequiredService<DataKepserverService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
