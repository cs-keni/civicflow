namespace CivicFlow.Domain.Enums;

public enum AuditAction
{
    Created,
    Updated,
    Deleted,
    StatusChanged,
    LoginSuccess,
    LoginFailed,
    PermissionDenied
}
