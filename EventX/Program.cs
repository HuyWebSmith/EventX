using EventX.Models;
using EventX.Repositories;
using EventX.Services;
using EventX.Services.Email;
using EventX.Services.VNPay;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHangfire(config => config.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddRazorPages();
builder.Services.AddScoped<IEventRepository, EFEventRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.AddScoped<QrCodeService>();
builder.Services.AddScoped<EventService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, EmailSender>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseHangfireDashboard("/hangfire"); 
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();


recurringJobManager.AddOrUpdate<EventService>(
    recurringJobId: "update-event-status", 
    methodCall: service => service.UpdateEventStatus(),
    cronExpression: Cron.Minutely(),  
    options: new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local  
    });


recurringJobManager.AddOrUpdate<TicketService>(
    recurringJobId: "update-ticket-status", 
    methodCall: service => service.UpdateTicketStatus(), 
    cronExpression: Cron.Minutely(), 
    options: new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local 
    });

recurringJobManager.AddOrUpdate<TicketService>(
    recurringJobId: "ReleaseExpiredHeldTickets",
    methodCall: service => service.ReleaseExpiredHeldTickets(),
    cronExpression: Cron.Minutely(),
    options: new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Utc

    });


app.UseHangfireServer();

app.UseSession();
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



app.Run();
