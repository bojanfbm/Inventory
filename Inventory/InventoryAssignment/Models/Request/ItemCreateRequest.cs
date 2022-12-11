namespace Api.Models.Request;

public class ItemCreateRequest
{
    public long CompanyPrefix { get; set; }
    public string CompanyName { get; set; }
    public int ItemReference { get; set; }
    public string ItemName { get; set; }

    public override string ToString()
    {
        return $"{nameof(CompanyPrefix)}:{CompanyPrefix}, {nameof(CompanyName)}:{CompanyName}, {nameof(ItemReference)}:{ItemReference}, {nameof(ItemName)}:{ItemName}";
    }
}