using Microsoft.EntityFrameworkCore;
using NutriPlanner.Components;
using NutriPlanner.Data;
using NutriPlanner.Models;
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

    if (!dbContext.Ingredients.Any())
    {
        Console.WriteLine("--- Génération des données de test ---");

        var huileOlive = new Ingredient { Nom = "Huile d'olive",   CaloriesParUnite = 8.8,  Unite = "ml"    };
        var farine     = new Ingredient { Nom = "Farine",          CaloriesParUnite = 3.6,  Unite = "g"     };
        var oeuf       = new Ingredient { Nom = "Oeuf",            CaloriesParUnite = 70,   Unite = "pièce" };
        var tomate     = new Ingredient { Nom = "Tomate",          CaloriesParUnite = 0.18, Unite = "g"     };
        var poulet     = new Ingredient { Nom = "Blanc de poulet", CaloriesParUnite = 1.65, Unite = "g"     };
        var pates      = new Ingredient { Nom = "Pâtes",           CaloriesParUnite = 3.7,  Unite = "g"     };
        var sucre      = new Ingredient { Nom = "Sucre",           CaloriesParUnite = 3.87, Unite = "g"     };
        var lait       = new Ingredient { Nom = "Lait",            CaloriesParUnite = 0.42, Unite = "ml"    };
        var harissa    = new Ingredient { Nom = "Harissa",         CaloriesParUnite = 0.5,  Unite = "g"     };
        var fromage    = new Ingredient { Nom = "Fromage",         CaloriesParUnite = 4.0,  Unite = "g"     };

        dbContext.Ingredients.AddRange(huileOlive, farine, oeuf, tomate, poulet,
                                       pates, sucre, lait, harissa, fromage);
        dbContext.SaveChanges();

        var shakshuka = new Recette
        {
            Titre = "Shakshuka", Description = "Oeufs pochés dans une sauce tomate épicée.",
            NombrePersonnes = 2, Categorie = "Déjeuner", TypeCuisine = "Tunisienne",
            RecetteIngredients = new List<RecetteIngredient>
            {
                new() { Ingredient = tomate,     Quantite = 300 },
                new() { Ingredient = oeuf,       Quantite = 4   },
                new() { Ingredient = harissa,    Quantite = 20  },
                new() { Ingredient = huileOlive, Quantite = 15  }
            }
        };

        var carbonara = new Recette
        {
            Titre = "Pâtes Carbonara", Description = "Pâtes crémeuses avec oeufs et fromage.",
            NombrePersonnes = 4, Categorie = "Dîner", TypeCuisine = "Italienne",
            RecetteIngredients = new List<RecetteIngredient>
            {
                new() { Ingredient = pates,   Quantite = 400 },
                new() { Ingredient = oeuf,    Quantite = 4   },
                new() { Ingredient = fromage, Quantite = 100 }
            }
        };

        var crepes = new Recette
        {
            Titre = "Crêpes", Description = "Crêpes légères au petit-déjeuner.",
            NombrePersonnes = 4, Categorie = "Petit-déjeuner", TypeCuisine = "Française",
            RecetteIngredients = new List<RecetteIngredient>
            {
                new() { Ingredient = farine, Quantite = 250 },
                new() { Ingredient = oeuf,   Quantite = 3   },
                new() { Ingredient = lait,   Quantite = 500 },
                new() { Ingredient = sucre,  Quantite = 30  }
            }
        };

        var pouletGrille = new Recette
        {
            Titre = "Poulet Grillé Méditerranéen", Description = "Poulet mariné aux herbes.",
            NombrePersonnes = 3, Categorie = "Dîner", TypeCuisine = "Méditerranéenne",
            RecetteIngredients = new List<RecetteIngredient>
            {
                new() { Ingredient = poulet,     Quantite = 600 },
                new() { Ingredient = huileOlive, Quantite = 30  },
                new() { Ingredient = tomate,     Quantite = 150 }
            }
        };

        dbContext.Recettes.AddRange(shakshuka, carbonara, crepes, pouletGrille);
        dbContext.SaveChanges();
    }

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

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