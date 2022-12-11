namespace Api.Models.Request;

public class InventoryCreateRequest
{
    public string InventoryId { get; set; }
    public string InventoryLocation { get; set; }
    public DateTimeOffset InventoryDate { get; set; }
    public List<string> Tags { get; set; }

    public override string ToString()
    {
        return $"{nameof(InventoryId)}:{InventoryId}, {nameof(InventoryLocation)}:{InventoryLocation}, {nameof(InventoryDate)}:{InventoryDate}, {nameof(Tags)}:{Tags}";
    }
}