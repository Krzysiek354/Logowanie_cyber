using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Cyber_zad_1_.Models.Context;
using Cyber_zad_1_.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext and Identity
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<DBContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();  // Important for Identity
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Login}/{id?}");
});

using var scope = app.Services.CreateScope();
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    if (!roleManager.RoleExistsAsync("Admin").Result)
    {
        var role = new IdentityRole("Admin");
        var roleResult = roleManager.CreateAsync(role).Result;
    }

    if (!roleManager.RoleExistsAsync("User").Result)
    {
        var role = new IdentityRole("User");
        var roleResult = roleManager.CreateAsync(role).Result;
    }

    // Dodawanie u¿ytkownika ADMIN, jeœli jeszcze nie istnieje
    var adminUserName = "ADMIN";
    var adminUser = userManager.FindByNameAsync(adminUserName).Result;
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminUserName,
            Email = "admin@example.com",
            IsFirstLogin = true,
            IsBlocked = false
        };

        var adminPassword = "InitialAdminPassword123!";  
        var createUserResult = userManager.CreateAsync(adminUser, adminPassword).Result;
        if (createUserResult.Succeeded)
        {
            userManager.AddToRoleAsync(adminUser, "Admin").Wait();
        }
        else
        {
           
            foreach (var error in createUserResult.Errors)
            {
                Console.WriteLine(error.Description);
            }
        }
    }
}

app.Run();