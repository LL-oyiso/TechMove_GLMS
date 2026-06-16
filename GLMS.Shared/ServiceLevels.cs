namespace GLMS.Shared;

public static class ServiceLevels
{
    public static readonly IReadOnlyList<string> All = new[]
    {
        "Bronze",
        "Silver",
        "Gold",
        "Platinum",
        "Enterprise"
    };

    public static bool IsValid(string? level) =>
        !string.IsNullOrWhiteSpace(level) &&
        All.Any(l => string.Equals(l, level, StringComparison.OrdinalIgnoreCase));
}
