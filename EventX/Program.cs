using EventX.Models;
using EventX.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHangfire(config => config.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();


builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddDefaultTokenProviders()
        .AddDefaultUI()
        .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.LogoutPath = "/Identity/Account/AccessDenied";
});


builder.Services.AddRazorPages();
builder.Services.AddScoped<IEventRepository, EFEventRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<EventService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseHangfireDashboard("/hangfire"); // ?? có th? truy c?p Hangfire Dashboard t?i /hangfire
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

// ??ng ký công vi?c ??nh k?
recurringJobManager.AddOrUpdate<EventService>(
    recurringJobId: "update-event-status",  // ID c?a công vi?c
    methodCall: service => service.UpdateEventStatus(),  // Ph??ng th?c c?n g?i
    cronExpression: Cron.Minutely(),  // Bi?u th?c cron: ch?y m?i phút
    options: new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local  // Thi?t l?p múi gi? ??a ph??ng
    });
app.UseHangfireServer();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();
app.MapStaticAssets();
app.UseStaticFiles(); // Cho phép truy c?p file t?nh t? wwwroot

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
