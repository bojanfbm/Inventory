using System.Collections.Immutable;
using Api.Models.Request;
using Api.Models.Response;
using BusinessLogic.Data;
using BusinessLogic.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services;

namespace API.Controllers;

[ApiController]
[Route("api")]
public class InventoryController : ControllerBase
{
    private readonly ILogger<InventoryController> _logger;
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _logger = logger;
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Endpoint for adding item
    /// </summary>
    /// <param name="request">ItemCreate request with required params</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Item id</returns>
    [HttpPost]
    [Route("/v1/product/create")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateItemAsync([FromBody] ItemCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{actor} called with {params}", $"{nameof(InventoryController)}-{nameof(CreateItemAsync)}", request);

        var item = new Item {CompanyName = request.CompanyName, Name = request.ItemName, CompanyPrefix = request.CompanyPrefix, Reference = request.ItemReference};

        await _inventoryService.AddItemAsync(item, cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Endpoint for adding inventory
    /// </summary>
    /// <param name="request">InventoryCreate request with required params</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Inventory id</returns>
    [HttpPost]
    [Route("/v1/inventory/create")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInventoryAsync([FromBody] InventoryCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{actor} called with {params}", $"{nameof(InventoryController)}-{nameof(CreateInventoryAsync)}", request);

        var inventory = new Inventory
        {
            Date = request.InventoryDate,
            InventoryId = request.InventoryId,
            Location = request.InventoryLocation,
            Tags = request.Tags
        };

        await _inventoryService.AddInventoryAsync(inventory, cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Get item count per inventory grouped by items
    /// </summary>
    /// <param name="inventoryId">Id of specific inventory</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Inventory count response</returns>
    [HttpGet]
    [Route("/v1/inventory/{inventoryId}/count")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetItemCountForInventoryAsync(string inventoryId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Info");

        var inventoryModel = await _inventoryService.GetProductCountByInventoryAsync(inventoryId, cancellationToken);
            
        var response = new InventoryCountResponse(inventoryModel.InventoryId, inventoryModel.Location, inventoryModel.Date, ItemCountResponses(inventoryModel.Items.ToImmutableList()));

        return Ok(response);
    }

    /// <summary>
    /// Get item count per inventory for specific date grouped by items
    /// </summary>
    /// <param name="inventoryDateOffset">Creation DateTime offset for inventory</param>
    /// <param name="cancellationToken"></param>
    /// <returns>InventoryCountSimpleResponse</returns>
    [HttpGet]
    [Route("/v1/inventory/count-by-date")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetProductCountForInventoryOnDateAsync([FromQuery] DateTimeOffset inventoryDateOffset, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Info");

        var items = await _inventoryService.GetItemsCountByInventoryDateAsync(inventoryDateOffset, cancellationToken);

        var response = new InventoryCountSimpleResponse(ItemCountResponses(items));

        return Ok(response);
    }

    /// <summary>
    /// Get item count for specific company grouped by items
    /// </summary>
    /// <param name="companyPrefix">Id of company</param>
    /// <param name="cancellationToken"></param>
    /// <returns>InventoryCountSimpleResponse</returns>
    [HttpGet]
    [Route("/v1/inventory/count-by-company")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetProductCountForCompanyAsync([FromQuery] long companyPrefix, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Info");

        var items = await _inventoryService.GetItemsCountByCompanyAsync(companyPrefix, cancellationToken);
            
        var response = new InventoryCountSimpleResponse(ItemCountResponses(items));

        return Ok(response);
    }

    private List<ItemCountResponse> ItemCountResponses(ImmutableList<ItemModel> items)
    {
        var responseItems = items.DistinctBy(o => new { o.CompanyPrefix, o.Reference })
            .Select(i => new ItemCountResponse(i.Name, i.CompanyName, i.Count)).ToList();

        return responseItems;
    }
}