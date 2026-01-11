namespace OsService.Services.Exceptions;

public sealed class ValidationException(string message) : Exception(message)
{
}