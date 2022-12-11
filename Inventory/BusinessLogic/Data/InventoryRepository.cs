using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Data;

public interface IInventoryRepository
{
    public Task<int> AddItemAsync(Item item, CancellationToken cancellationToken);
    public Task<int> AddInventoryAsync(Inventory inventory, CancellationToken cancellationToken);
    public Task<Inventory?> GetInventoryAsync(string id, CancellationToken cancellationToken);
    public Task<IEnumerable<Inventory>> GetAllInventoriesAsync(CancellationToken cancellationToken);
    public Task<IEnumerable<Item>> GetAllItemsAsync(CancellationToken cancellationToken);
    public Task<Item?> GetItemByReferenceAsync(int itemReference, long companyPrefix, CancellationToken cancellationToken);
}

public class InventoryRepository : IInventoryRepository
{
    public InventoryRepository()
    {
        using var context = new InventoryDbContext();

        var items = LoadCsvData();

        context.Items.AddRange(items);
        context.SaveChanges();
    }

    private static IEnumerable<Item> LoadCsvData()
    {
        // parse CSV to DB
        var items = File.ReadAllLines("data.csv")
            .Skip(1)
            .Select(FromCsv)
            .ToList();

        return items;

        Item FromCsv(string csvLine)
        {
            var values = csvLine.Split(';');

            //validate
            const string numericPattern = @"^\d+$";

            var _ = Regex.IsMatch(values[0], numericPattern);
            _ = Regex.IsMatch(values[2], numericPattern);

            return new Item
            {
                CompanyName = values[1],
                Name = values[3],
                CompanyPrefix = Convert.ToInt64(values[0]),
                Reference = Convert.ToInt32(values[2])
            };
        }
    }

    public async Task<Item?> GetItemByReferenceAsync(int itemReference, long companyPrefix, CancellationToken cancellationToken)
    {
        await using var context = new InventoryDbContext();

        var item = await context.Items.FirstOrDefaultAsync(i => i.Reference.Equals(itemReference) && i.CompanyPrefix.Equals(companyPrefix), cancellationToken);

        return item;
    }

    public async Task<int> AddItemAsync(Item item, CancellationToken cancellationToken)
    {
        await using var context = new InventoryDbContext();

        var result = await context.Items.AddAsync(item, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return result.Entity.Id;
    }

    public async Task<int> AddInventoryAsync(Inventory inventory, CancellationToken cancellationToken)
    {
        await using var context = new InventoryDbContext();
            
        var result = await context.Inventories.AddAsync(inventory, cancellationToken);
            
        await context.SaveChangesAsync(cancellationToken);

        return result.Entity.Id;
    }

    public async Task<Inventory?> GetInventoryAsync(string id, CancellationToken cancellationToken)
    {
        await using var context = new InventoryDbContext();

        var inventory = await context.Inventories.FirstOrDefaultAsync(i => i.InventoryId.Equals(id), cancellationToken);

        return inventory;
    }
        
    public async Task<IEnumerable<Inventory>> GetAllInventoriesAsync(CancellationToken cancellationToken)
    {
        await using var context = new InventoryDbContext();

        var inventories = await context.Inventories.ToListAsync(cancellationToken);

        return inventories;
    }

    public async Task<IEnumerable<Item>> GetAllItemsAsync(CancellationToken cancellationToken)
    {
        await using var context = new InventoryDbContext();

        var items = await context.Items.ToListAsync(cancellationToken);

        return items;
    }
}