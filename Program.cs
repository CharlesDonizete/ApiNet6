using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>();

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapGet("/", () => "Hello World 4!");

app.MapPost("/products", (Product product) =>
{
    ProductRepository.Add(product);
    return Results.Created($"/products/{product.Code}", product.Code);
});

app.MapGet("/products/{code}", ([FromRoute] string code) =>
{
    var product = ProductRepository.GetBy(code);
    if (product != null) return Results.Ok(product);
    return Results.NotFound();
});

app.MapPut("/products", (Product product) =>
{
    var productSaved = ProductRepository.GetBy(product.Code);
    productSaved.Name = product.Name;
    return Results.Ok();
});

app.MapDelete("/products/{code}", ([FromRoute] string code) =>
{
    var productSaved = ProductRepository.GetBy(code);
    ProductRepository.Remove(productSaved);
    return Results.Ok();
});

if (app.Environment.IsDevelopment())
    app.MapGet("/configuration/database", (IConfiguration configuration) =>
    {
        return Results.Ok($"{configuration["database:connection"]}:{configuration["database:port"]}");
    });

app.Run();

public static class ProductRepository
{
    public static List<Product> Products { get; set; }

    public static void Init(IConfiguration configuration)
    {
        var products = configuration.GetSection("Products").Get<List<Product>>();
        Products = products;
    }

    public static void Add(Product product)
    {
        if (Products == null)
        {
            Products = new List<Product>();
        }

        Products.Add(product);
    }

    public static Product GetBy(string code) => Products.FirstOrDefault(p => p.Code == code);
    public static void Remove(Product product) => Products.Remove(product);
}

public class Product
{
    public string Code { get; set; }
    public string Name { get; set; }
}

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Data Source = DESKTOP-U0N4N3T\\SQLEXPRESS;Initial Catalog = Products; Integrated Security = False; User ID = sa;Password=123; Connect Timeout = 15; Encrypt = False; TrustServerCertificate = False");
}