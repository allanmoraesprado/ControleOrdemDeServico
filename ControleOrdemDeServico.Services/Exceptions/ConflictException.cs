namespace OsService.Services.Exceptions;

public sealed class ConflictException(string message) : Exception(message)
{
}