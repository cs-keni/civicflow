namespace CivicFlow.Application.Common;

public record ApiError(string Message, IEnumerable<string>? Errors = null);
