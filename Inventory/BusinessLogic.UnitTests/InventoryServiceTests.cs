using AutoFixture;
using AutoFixture.AutoNSubstitute;
using BusinessLogic.Data;
using BusinessLogic.Models.Entities;
using BusinessLogic.Models.Exceptions;
using BusinessLogic.Services;
using BusinessLogic.Utility;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace BusinessLogic.UnitTests;

public class InventoryServiceTests
{
    private readonly IFixture _fixture;
    private readonly IInventoryRepository _repository;
    private readonly IInventoryService _sut;

    public InventoryServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });

        _repository = _fixture.Create<IInventoryRepository>();

        //Used real service to be able to use real data and test parsing (in real scenario would separate unit tests/add integration tests for whole flow)
        var parser = new Sgtin96Parser();
        
        _sut = new InventoryService(_repository, parser, NullLogger<InventoryService>.Instance);
    }
        
    [Fact]
    public async void AddItemAsync_CalledWithParameter_AddsItemToDatabase()
    {
        //Arrange
        var item = _fixture.Create<Item>();
        _repository.GetItemByReferenceAsync(default, default, CancellationToken.None)
            .ReturnsNullForAnyArgs();

        //Act
        await _sut.AddItemAsync(item, CancellationToken.None);

        //Assert
        await _repository.Received().AddItemAsync(item,Arg.Any<CancellationToken>());
    }

    [Fact]
    public async void AddItemAsync_IdempotentCall_ReturnItem()
    {
        //Arrange
        var item = _fixture.Create<Item>();

        //Act
        await _sut.AddItemAsync(item, CancellationToken.None);

        //Assert
        await _repository.DidNotReceiveWithAnyArgs().AddItemAsync(item, Arg.Any<CancellationToken>());
        await _repository.GetItemByReferenceAsync(default, default, CancellationToken.None);
    }

    [Fact]
    public async void AddInventoryAsync_IdempotentCall_AddsItemToDatabase()
    {
        //Arrange
        var inventory = _fixture.Create<Inventory>();
        //Act
        await _sut.AddInventoryAsync(inventory, CancellationToken.None);

        //Assert
        await _repository.DidNotReceiveWithAnyArgs().AddInventoryAsync(inventory, Arg.Any<CancellationToken>());
        await _repository.Received().GetInventoryAsync(inventory.InventoryId, CancellationToken.None);
    }

    [Fact]
    public async void GetProductCountByInventoryAsync_CalledWithValidInventoryId_ReturnsCount()
    {
        //Arrange
        var inventoryId = _fixture.Create<string>();

        var inventoryDao = _fixture.Create<Inventory>();
        inventoryDao.Tags = new List<string>()
        {
            "3019B9368A10A6C022E76FF5",
            "3089F8221B8D6B800E501E97"
        };

        _repository.GetInventoryAsync(Arg.Any<string>(), default)
            .ReturnsForAnyArgs(inventoryDao);
            
        var item1 = _fixture.Create<Item>();
        item1.CompanyPrefix = 8457952141;
        item1.Reference = 430;
        item1.Name = _fixture.Create<string>();

        var item2 = _fixture.Create<Item>();
        item2.CompanyPrefix = 451802;
        item2.Reference = 2638491;

        var itemsDao = _fixture.Create<List<Item>>();
        itemsDao.AddRange(new List<Item> { item1,item2 });

        _repository.GetAllItemsAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(itemsDao);

        //Act
        var result = await _sut.GetProductCountByInventoryAsync(inventoryId, CancellationToken.None);

        //Assert
        using var scope = new AssertionScope();

        result.InventoryId.Should().Be(inventoryDao.InventoryId);
        result.Date.Should().Be(inventoryDao.Date);
        result.Location.Should().Be(inventoryDao.Location);

        AssertItemInList(result.Items, item1, 1);
        AssertItemInList(result.Items, item2, 1);

        await _repository.Received().GetInventoryAsync(inventoryId, Arg.Any<CancellationToken>());
        await _repository.Received().GetAllItemsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async void GetProductCountByInventoryAsync_InventoryNotFound_ThrowsError()
    {
        //Arrange
        var inventoryId = _fixture.Create<string>();

        _repository.GetInventoryAsync(Arg.Any<string>(), default)
            .ReturnsNullForAnyArgs();

        //Act
        Func<Task> call = async () => await _sut.GetProductCountByInventoryAsync(inventoryId, CancellationToken.None);
            
        //Assert
        using var scope = new AssertionScope();

        await call.Should().ThrowAsync<EntityNotFound>();

        await _repository.Received().GetInventoryAsync(inventoryId, Arg.Any<CancellationToken>());
        await _repository.DidNotReceiveWithAnyArgs().GetAllItemsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async void GetItemsCountByInventoryDateAsync_CalledWithValidInventoryDate_ReturnsCount()
    {
        //Arrange
        var inventoryDateOffset = _fixture.Create<DateTimeOffset>();
            
        var inventoryDao = _fixture.Create<Inventory>();
        inventoryDao.Date = inventoryDateOffset;
        inventoryDao.Tags = new List<string>()
        {
            "3019B9368A10A6C022E76FF5",
            "3089F8221B8D6B800E501E97"
        };
        var inventoriesListDao = _fixture.Create<List<Inventory>>();
        inventoriesListDao.Add(inventoryDao);
            
        _repository.GetAllInventoriesAsync( default)
            .ReturnsForAnyArgs(inventoriesListDao);

        var item1 = _fixture.Create<Item>();
        item1.CompanyPrefix = 8457952141;
        item1.Reference = 430;

        var item2 = _fixture.Create<Item>();
        item2.CompanyPrefix = 451802;
        item2.Reference = 2638491;

        var itemsDao = _fixture.Create<List<Item>>();
        itemsDao.AddRange(new List<Item> { item1, item2 });

        _repository.GetAllItemsAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(itemsDao);

        //Act
        var result = await _sut.GetItemsCountByInventoryDateAsync(inventoryDateOffset, CancellationToken.None);

        //Assert
        using var scope = new AssertionScope();

        AssertItemInList(result, item1, 1);
        AssertItemInList(result, item2, 1);

        await _repository.Received().GetAllInventoriesAsync(Arg.Any<CancellationToken>());
        await _repository.Received().GetAllItemsAsync(Arg.Any<CancellationToken>());
    }
        
    [Fact]
    public async void GetItemsCountByCompanyAsync_CalledForCompanyPrefix_ReturnsCount()
    {
        //Arrange
        const long companyPrefix = 8457952141;

        var inventoryDao = _fixture.Create<Inventory>();
        inventoryDao.Tags = new List<string>
        {
            "3019B9368A10A6C022E76FF5",
            "3089F8221B8D6B800E501E97"
        };
        var inventoriesListDao = new List<Inventory> { inventoryDao };

        _repository.GetAllInventoriesAsync(default)
            .ReturnsForAnyArgs(inventoriesListDao);

        var item = _fixture.Create<Item>();
        item.CompanyPrefix = companyPrefix;
        item.Reference = 430;

        var itemsDao = _fixture.Create<List<Item>>();
        itemsDao.Add(item);

        _repository.GetAllItemsAsync(Arg.Any<CancellationToken>()).ReturnsForAnyArgs(itemsDao);

        //Act
        var result = await _sut.GetItemsCountByCompanyAsync(companyPrefix, CancellationToken.None);

        //Assert
        using var scope = new AssertionScope();
        AssertItemInList(result, item, 1);

        await _repository.Received().GetAllInventoriesAsync(Arg.Any<CancellationToken>());
        await _repository.Received().GetAllItemsAsync(Arg.Any<CancellationToken>());
    }

    private static void AssertItemInList(IEnumerable<ItemModel> result, Item item, short count)
    {
        result.Should().Contain(i => i.CompanyPrefix.Equals(item.CompanyPrefix)
                                     && i.Name!.Equals(item.Name)
                                     && i.Count == count
                                     && i.CompanyName!.Equals(item.CompanyName));
    }
}