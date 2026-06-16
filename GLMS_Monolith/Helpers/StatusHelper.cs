namespace GLMS_Monolith.Helpers;

// Formats enum status values for display. Works with any enum (uses the name),
// so it is decoupled from the specific enum type.
public static class StatusHelper
{
    public static string Label(object? status) => status?.ToString() switch
    {
        "OnHold"     => "On Hold",
        "InProgress" => "In Progress",
        var s        => s ?? string.Empty
    };
}
