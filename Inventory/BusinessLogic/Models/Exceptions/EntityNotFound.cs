using System.Net;

namespace BusinessLogic.Models.Exceptions;

public class EntityNotFound : Exception
{
    public int StatusCode => (int)HttpStatusCode.NotFound;

    public EntityNotFound(string message = "Entity not found!") : base(message)
    {
    }
}