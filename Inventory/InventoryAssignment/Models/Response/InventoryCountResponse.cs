namespace Api.Models.Response;

public class InventoryCountResponse : InventoryCountSimpleResponse
{
    public InventoryCountResponse(string inventoryId, string location, DateTimeOffset date, List<ItemCountResponse> items)
        :base(items)
    {
        InventoryId = inventoryId;
        Location = location;
        Date = date;
    }

    public string InventoryId { get; set; }
    public string Location { get; set; }
    public DateTimeOffset Date { get; set; }
}