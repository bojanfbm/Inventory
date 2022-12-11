namespace Api.Models.Response;

public class InventoryCountSimpleResponse
{
    public InventoryCountSimpleResponse(List<ItemCountResponse> items)
    {
        Items = items;
    }

    public List<ItemCountResponse> Items { get; set; }
}