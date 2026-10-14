namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request window policy.</summary>
public static class LeaveRequestWindowPolicy
{
    public static string LeaveRequestWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a leave request window code in its canonical LRW- form.</summary>
    public static string NormalizeLeaveRequestWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
