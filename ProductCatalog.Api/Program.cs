using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CatalogDbConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
object value = builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Crée la base de données et le schéma si nécessaire
    db.Database.EnsureCreated();
}

// 1. Endpoint d'observabilité / Healthcheck applicatif
app.MapGet("/health", () =>
    Results.Ok(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow
    }))
    .WithName("HealthCheck");

// 2. Endpoint : liste des produits
app.MapGet("/api/products", async (AppDbContext db) =>
    await db.Products
        .AsNoTracking()
        .ToListAsync())
    .WithName("GetAllProducts");

// 3. Endpoint : ajout d'un produit
app.MapPost("/api/products", async (Product product, AppDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/products/{product.Id}",
        product
    );
})
.WithName("CreateProduct");

app.Run();