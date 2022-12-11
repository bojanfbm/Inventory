using BusinessLogic.Models;

public interface ISgtinParser
{
    Sgtin TryParse(string tag);
}