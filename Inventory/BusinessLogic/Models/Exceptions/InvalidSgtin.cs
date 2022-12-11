using System.Net;

namespace BusinessLogic.Models.Exceptions;

public class InvalidSgtin : Exception
{
    public int StatusCode => (int)HttpStatusCode.Conflict; // or 422 - Unprocessable Entity

    public InvalidSgtin(string message = "Invalid sgting") : base(message)
    {
    }
}