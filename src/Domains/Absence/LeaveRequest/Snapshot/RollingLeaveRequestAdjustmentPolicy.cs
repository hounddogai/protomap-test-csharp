namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling leave request adjustment policy.</summary>
public static class RollingLeaveRequestAdjustmentPolicy
{
    public static string RollingLeaveRequestAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a rolling leave request adjustment code in its canonical RLRA- form.</summary>
    public static string NormalizeRollingLeaveRequestAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RLRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RLRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
