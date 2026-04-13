namespace SysOpCore.Models;

public sealed class ActionCard
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string CommandKey { get; init; }
    public bool IsDangerous { get; init; }
}
