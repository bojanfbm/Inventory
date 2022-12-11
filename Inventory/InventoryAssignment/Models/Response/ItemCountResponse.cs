namespace Api.Models.Response;

public class ItemCountResponse
{
    public ItemCountResponse(string? name, string? companyName, int count)
    {
        Name = name;
        CompanyName = companyName;
        Count = count;
    }

    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public int Count { get; set; }
}