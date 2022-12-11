namespace BusinessLogic.Models.Entities;

public class ItemModel
{
    public long Reference { get; set; }
    public string? Name { get; set; }
    public long CompanyPrefix { get; set; }
    public string? CompanyName { get; set; }
    public int Count { get; set; }
}