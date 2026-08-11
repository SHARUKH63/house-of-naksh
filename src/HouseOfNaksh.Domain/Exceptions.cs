namespace HouseOfNaksh.Domain;

public abstract class DomainException(string message) : Exception(message);

public class NotFoundException(string resource, object key)
    : DomainException($"{resource} with id '{key}' was not found")
{
    public string Resource { get; } = resource;
}

public class ConflictException(string message) : DomainException(message);

public class BusinessRuleException(string message) : DomainException(message);
