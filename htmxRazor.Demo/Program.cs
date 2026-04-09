using htmxRazor.Demo.Hubs;
using htmxRazor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddHostedService<DemoClockService>();
builder.Services.AddhtmxRazor(options =>
{
    options.DefaultTheme = "light";
    options.IncludeHtmxScript = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UsehtmxRazor();
app.UseRouting();
app.MapRazorPages();
app.MapHub<DemoHub>("/demoHub");

app.Run();
