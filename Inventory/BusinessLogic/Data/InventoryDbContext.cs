using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Data;

public class InventoryDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(databaseName: "InventoryAssignmentDb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inventory>()
            .Property(b => b.Tags)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)default)); ;
    }

    public DbSet<Item> Items { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
}

public class Item
{
    public int Id { get; set; }
    public int Reference { get; set; }
    public string Name { get; set; }
    public long CompanyPrefix { get; set; }
    public string CompanyName { get; set; }
}

public class Inventory
{
    public int Id { get; set; }
    public string InventoryId { get; set; }
    public string Location { get; set; }
    public DateTimeOffset Date { get; set; }
    public List<string> Tags { get; set; }
}