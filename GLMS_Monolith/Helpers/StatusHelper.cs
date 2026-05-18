namespace GLMS_Monolith.Helpers;

public static class StatusHelper
{
    public static string Label(object? status) => status?.ToString() switch
    {
        "OnHold"     => "On Hold",
        "InProgress" => "In Progress",
        var s        => s ?? string.Empty
    };
}
