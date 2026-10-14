namespace Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Functions of leave request item policy.</summary>
public static class LeaveRequestItemPolicy
{
    public static int LeaveRequestItemSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a leave request item code in its canonical LRI- form.</summary>
    public static string NormalizeLeaveRequestItemCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRI-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRI-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
