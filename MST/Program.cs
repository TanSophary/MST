using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MST.Data;
using MST.Models;
using MST.Services;
using MST.Data;
using MST.Models;
using MST.Services;
using MST;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient("8498115829:AAHvcNPD3edo8pcDRyXc-gGvSS023fjsISQ"));
// Program.cs
builder.Services.AddLogging(logging => logging.AddConsole());

builder.Services.Configure<EmailService>(builder.Configuration.GetSection("EmailSetting"));
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

await SeedService.SeedDatabase(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
