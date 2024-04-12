using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using testingINTEX.Data;
using testingINTEX.Models;
//Grant CSP stuff
using Microsoft.AspNetCore.Builder;
using NWebsec.AspNetCore.Mvc;
//Grant Rate limiting stuff
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
// Grant HSTS Stuff
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<IntexpostgresContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

/////////
// builder.Services.AddIdentity<AppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddScoped<IIntexRepository, EFIntexRepository>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();


// Replace IdentityUser with your custom user class if you have one.
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<ApplicationDbContext>();
// Use your custom context here

//

builder.Services.AddControllersWithViews();

// Add session configuration
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true; // make the session cookie essential
});

//Grant rate limiting stuff
// Add rate limiting services
// Add processing strategy service
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimiting"));

//Grant Rate Limiting stuff
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(4);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));
//GRant rate limiting
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();

//Grant HSTS Stuff
builder.Services.AddHsts(options =>
{
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//HSTS Stuff Grant
app.UseHttpsRedirection();

//Grant rate limiting
app.UseIpRateLimiting();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();

app.UseSession(); // Add session middleware

app.MapControllerRoute("pagination", "Projects/{PageNum}", new { Controller = "Home", action = "LoggedInLandingPage" });
app.MapDefaultControllerRoute();
app.MapRazorPages();

//Grant CSP Stuff
app.UseCsp(options => options
    .ScriptSources(s => s.Self().UnsafeInline().UnsafeEval())  // Allow inline scripts and eval
    .FontSources(s => s.Self())                 // Allow fonts from the same origin
    .UpgradeInsecureRequests()                  // Upgrade HTTP requests to HTTPS
);

//Grant HSTS Stuff
app.UseHsts();

app.Run();


