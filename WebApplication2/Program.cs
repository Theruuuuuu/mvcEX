using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using BulkyBook.Utility;
using Stripe;
using BulkyBook.DataAccess.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddIdentity<IdentityUser,IdentityRole>(/*option=>option.SignIn.RequireConfirmedEmail = true*/).AddDefaultUI().AddDefaultTokenProviders() //Email相關
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Lockout settings.
    options.Password.RequireNonAlphanumeric = false;    
});

//builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "1307333150166339";
    option.AppSecret = "8605b32d3d916b1d8d2927ed6bea529c";
});
builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = $"/Identity/Account/Login";
    option.LogoutPath = $"/Identity/Account/Logout";
    option.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(option => 
{
    option.IdleTimeout = TimeSpan.FromSeconds(100);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
SeedDatabase();
app.UseAuthentication();

app.UseAuthorization();
app.UseSession();
app.MapRazorPages(); //Razor才找的到View
app.MapControllerRoute(
    name: "default",
    pattern: "{Area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}