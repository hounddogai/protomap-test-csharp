namespace Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Functions of weighted leave request adjustment policy.</summary>
public static class WeightedLeaveRequestAdjustmentPolicy
{
    public static int WeightedLeaveRequestAdjustmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a weighted leave request adjustment code in its canonical WLRA- form.</summary>
    public static string NormalizeWeightedLeaveRequestAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WLRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WLRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
