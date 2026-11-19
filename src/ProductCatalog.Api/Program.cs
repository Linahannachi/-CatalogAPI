
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString =
    builder.Configuration.GetConnectionString("CatalogDbConnection")
    ?? "Server=localhost,1433;Database=CatalogDb;User Id=sa;Password=DevOpsPass2026!#;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Créer la base et la table si elles n'existent pas
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Vérifier que l'API fonctionne
app.MapGet("/health", () =>
    Results.Ok(new { status = "Healthy" }))
    .WithName("Health");

// Lire tous les produits
app.MapGet("/api/products", async (AppDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    return Results.Ok(products);
})
.WithName("GetProducts");

// Ajouter un produit
app.MapPost("/api/products", async (
    CreateProductRequest request,
    AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0)
    {
        return Results.BadRequest("Nom invalide ou prix négatif.");
    }

    var product = new Product
    {
        Name = request.Name,
        Description = request.Description,
        Price = request.Price
    };

    db.Products.Add(product);
    await db.SaveChangesAsync();

    return Results.Created($"/api/products/{product.Id}", product);
})
.WithName("CreateProduct");

app.Run();

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price);

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
}
