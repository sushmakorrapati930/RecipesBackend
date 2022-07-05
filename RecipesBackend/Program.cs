using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<RecipeDb>(opt => opt.UseInMemoryDatabase("RecipeList"));
var app = builder.Build();

app.MapGet("/api", () => "Hello World!");

app.MapGet("/api/recipes", async (RecipeDb db) =>
    await db.Recipes.ToListAsync());

//app.MapGet("/recepies/complete", async (RecipeDb db) =>
//    await db.Recipes.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/api/recipes/{id}", async (int id, RecipeDb db) =>
    await db.Recipes.FindAsync(id)
        is Recipe recipe
            ? Results.Ok(recipe)
            : Results.NotFound());

app.MapPost("/api/recipes", async (Recipe recipe, RecipeDb db) =>
{
    Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    recipe.Id = unixTimestamp;
    recipe.Added = DateTime.Now.ToString();
    recipe.Modified = DateTime.Now.ToString();
    db.Recipes.Add(recipe);
    await db.SaveChangesAsync();

    return Results.Created($"/api/recipes/{recipe.Id}", recipe);
});

app.MapPut("/api/recipes/{id}", async (int id,Recipe recipe, RecipeDb db) =>
{
    var recipeItem = await db.Recipes.FindAsync(id);

    if (recipeItem is null) return Results.NotFound();

    recipeItem.RecipeName = recipe.RecipeName;
    recipeItem.Ingredients = recipe.Ingredients;
    recipeItem.Instructions = recipe.Instructions;
    recipeItem.ServingSize = recipe.ServingSize;
    recipeItem.Category = recipe.Category;
    recipeItem.Notes = recipe.Notes;
    recipe.Modified = DateTime.Now.ToString();
    await db.SaveChangesAsync();
   

    return Results.Accepted($"/api/recipes/{recipe.Id}", recipe);
});

app.MapDelete("/api/recipes/{id}", async (int id, RecipeDb db) =>
{
    if (await db.Recipes.FindAsync(id) is Recipe todo)
    {
        db.Recipes.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

app.Run();

enum RecipeCategory
{
    Dessert,
    MainCourse,
    Appetizer,
    Soup
}
class Recipe
{
    public int Id { get; set; }
    public string? RecipeName { get; set; }
    public string? Ingredients { get; set; }
    public string? Instructions { get; set; }
    public int? ServingSize { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public string? Added { get; set; }
    public string? Modified { get; set; }

}

class RecipeDb : DbContext
{
    public RecipeDb(DbContextOptions<RecipeDb> options)
        : base(options) { }

    public DbSet<Recipe> Recipes=> Set<Recipe>();
}