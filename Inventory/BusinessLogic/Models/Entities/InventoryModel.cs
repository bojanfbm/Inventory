namespace BusinessLogic.Models.Entities;

public class InventoryModel
{
    public string InventoryId { get; set; }
    public string Location { get; set; }
    public DateTimeOffset Date { get; set; }
    public List<ItemModel> Items { get; set; }
}