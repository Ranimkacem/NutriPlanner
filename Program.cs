using Microsoft.EntityFrameworkCore;
using NutriPlanner.Components;
using NutriPlanner.Data;
using NutriPlanner.Services;
using Radzen;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IRecetteService, RecetteService>();
builder.Services.AddRadzenComponents();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
});

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services   = scope.ServiceProvider;
    var dbContext  = services.GetRequiredService<AppDbContext>();

    dbContext.Database.EnsureCreated();


    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (!await roleManager.RoleExistsAsync("User"))
        await roleManager.CreateAsync(new IdentityRole("User"));

    if (await userManager.FindByEmailAsync("admin@nutriplanner.com") == null)
    {
        var admin = new IdentityUser
        {
            UserName = "admin@nutriplanner.com",
            Email    = "admin@nutriplanner.com"
        };
        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.MapPost("/api/auth/login", async (
    [FromServices] SignInManager<IdentityUser> signInManager,
    [FromForm] string email,
    [FromForm] string password) =>
{
    var result = await signInManager.PasswordSignInAsync(email, password, false, false);
    if (result.Succeeded) return Results.Redirect("/dashboard");
    return Results.Redirect("/login?error=Email+ou+mot+de+passe+incorrect");
}).DisableAntiforgery();

app.MapPost("/api/auth/register", async (
    [FromServices] UserManager<IdentityUser>  userManager,
    [FromServices] SignInManager<IdentityUser> signInManager,
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] string confirmPassword) =>
{
    if (password != confirmPassword)
        return Results.Redirect("/register?error=Les+mots+de+passe+ne+correspondent+pas");

    if (await userManager.FindByEmailAsync(email) != null)
        return Results.Redirect("/register?error=Cet+email+est+déjà+utilisé");

    var user   = new IdentityUser { UserName = email, Email = email };
    var result = await userManager.CreateAsync(user, password);

    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, "User");
        await signInManager.SignInAsync(user, isPersistent: false);
        return Results.Redirect("/dashboard");
    }

    var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
    return Results.Redirect($"/register?error={Uri.EscapeDataString(errors)}");
}).DisableAntiforgery();

app.MapPost("/api/auth/logout", async (
    [FromServices] SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
}).DisableAntiforgery();

app.Run();