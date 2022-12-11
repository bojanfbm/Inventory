using System.Collections.Immutable;
using BusinessLogic.Data;
using BusinessLogic.Models.Entities;
using BusinessLogic.Models.Exceptions;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public interface IInventoryService
{
    Task<int> AddItemAsync(Item item, CancellationToken cancellationToken);

    Task<int> AddInventoryAsync(Inventory inventory, CancellationToken cancellationToken);

    Task<InventoryModel> GetProductCountByInventoryAsync(string inventoryId, CancellationToken cancellationToken);

    Task<ImmutableList<ItemModel>> GetItemsCountByInventoryDateAsync(DateTimeOffset inventoryDateOffset, CancellationToken cancellationToken);

    Task<ImmutableList<ItemModel>> GetItemsCountByCompanyAsync(long companyPrefix, CancellationToken cancellationToken);
}

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;
    private readonly ISgtinParser _sgtinParser;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IInventoryRepository repository, ISgtinParser sgtinParser, ILogger<InventoryService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _sgtinParser = sgtinParser ?? throw new ArgumentNullException(nameof(sgtinParser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> AddItemAsync(Item item, CancellationToken cancellationToken)
    {
        var idempotentItem = await _repository.GetItemByReferenceAsync(item.Reference, item.CompanyPrefix, cancellationToken);
        if (idempotentItem is not null)
        {
            return idempotentItem.Id;
        }

        return await _repository.AddItemAsync(item, cancellationToken);
    }

    public async Task<int> AddInventoryAsync(Inventory inventory, CancellationToken cancellationToken)
    {
        var idempotentItem = await _repository.GetInventoryAsync(inventory.InventoryId, cancellationToken);
        if (idempotentItem is not null)
        {
            return idempotentItem.Id;
        }

        return await _repository.AddInventoryAsync(inventory, cancellationToken);
    }
        
    public async Task<InventoryModel> GetProductCountByInventoryAsync(string inventoryId, CancellationToken cancellationToken)
    {
        var inventoryDao = await _repository.GetInventoryAsync(inventoryId, cancellationToken);

        if (inventoryDao is null)
        {
            var exc = new EntityNotFound($"Inventory with {inventoryId} not found!");
            _logger.LogError(exc, "Inventory[{inventoryId}] not found", inventoryId);
                
            throw exc;
        }

        //Test the speed if parallel is needed.
        //var watchForParallel = Stopwatch.StartNew();
        //var sgtins = new List<Sgtin>();
        //Parallel.ForEach(inventoryDao.Tags, tag =>
        //{
        //    sgtins.Add(Sgtin96Parser.TryParseSgtin96(tag));
        //});
        //watchForParallel.Stop();
        //Console.WriteLine("parallel: {0}", watchForParallel.ElapsedMilliseconds);

        var sgtingList = inventoryDao.Tags.Select(_sgtinParser.TryParse).ToList();

        var itemsDao = (await _repository.GetAllItemsAsync(cancellationToken)).ToImmutableList();
            
        var enumerable = itemsDao.ToArray();
        var mappedItems = sgtingList.GroupBy(i => i.ItemReference).Select(g =>
        {
            return new ItemModel
            {
                CompanyPrefix = g.First().CompanyPrefix,
                CompanyName = enumerable.FirstOrDefault(i => i.CompanyPrefix == g.First().CompanyPrefix && i.Reference == g.First().ItemReference)?.CompanyName,
                Name = enumerable.FirstOrDefault(i => i.CompanyPrefix == g.First().CompanyPrefix && i.Reference == g.First().ItemReference)?.Name,
                Reference = g.Key,
                Count = g.Count()
            };
        }).ToList();

        var model = new InventoryModel
        {
            Date = inventoryDao.Date,
            InventoryId = inventoryDao.InventoryId,
            Location = inventoryDao.Location,
            Items = mappedItems
        };
            
        return model;
    }

    public async Task<ImmutableList<ItemModel>> GetItemsCountByInventoryDateAsync(DateTimeOffset inventoryDateOffset, CancellationToken cancellationToken)
    {
        var inventoriesDao = await _repository.GetAllInventoriesAsync(cancellationToken);

        var inventoriesByDate = inventoriesDao.Where(i => i.Date.Date == inventoryDateOffset.Date).ToImmutableList();

        var itemsDao = (await _repository.GetAllItemsAsync(cancellationToken)).ToImmutableList();
           
        var sgtinList = inventoriesByDate.SelectMany(u => u.Tags.Select(_sgtinParser.TryParse).ToList());

        var items = sgtinList.GroupBy(i => i.ItemReference)
            .Select(g => new ItemModel
            {
                CompanyPrefix = g.First().CompanyPrefix,
                CompanyName = itemsDao.FirstOrDefault(i => i.CompanyPrefix.Equals(g.First().CompanyPrefix) && i.Reference == g.First().ItemReference)?.CompanyName,
                Name = itemsDao.FirstOrDefault(i => i.CompanyPrefix.Equals(g.First().CompanyPrefix) && i.Reference == g.First().ItemReference)?.Name,
                Reference = g.First().ItemReference,
                Count = g.Count()
            }).ToImmutableList();
            
        return items;
    }

    public async Task<ImmutableList<ItemModel>> GetItemsCountByCompanyAsync(long companyPrefix, CancellationToken cancellationToken)
    {
        var inventoriesDao = await _repository.GetAllInventoriesAsync(cancellationToken);
          
        var itemsDao = (await _repository.GetAllItemsAsync(cancellationToken)).ToImmutableList();
            
        var sgtinList = inventoriesDao.SelectMany(u => u.Tags.Select(_sgtinParser.TryParse)
            .Where(c => c.CompanyPrefix.Equals(companyPrefix))).ToImmutableList();
            
        var items = sgtinList.GroupBy(i => i.ItemReference)
            .Select(g => new ItemModel
            {
                CompanyPrefix = g.First().CompanyPrefix,
                CompanyName = itemsDao.FirstOrDefault(i => i.Reference == g.First().ItemReference)?.CompanyName,
                Name = itemsDao.FirstOrDefault(i => i.Reference == g.First().ItemReference)?.Name,
                Reference = g.Key,
                Count = g.Count()
            }).ToImmutableList();
            
        return items;
    }
}