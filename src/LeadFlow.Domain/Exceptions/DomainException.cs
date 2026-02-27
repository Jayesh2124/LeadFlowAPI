namespace LeadFlow.Domain.Exceptions;

public class DomainException(string message) : Exception(message);

public class EmailTaskStateException(string message) : DomainException(message);

public class NotFoundException(string entity, object id)
    : DomainException($"{entity} with id '{id}' was not found.");
