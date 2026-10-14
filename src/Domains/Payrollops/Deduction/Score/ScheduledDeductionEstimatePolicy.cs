namespace Acme.Workforce.Payrollops.Deduction.Score;

/// <summary>Functions of scheduled deduction estimate policy.</summary>
public static class ScheduledDeductionEstimatePolicy
{
    public static int ScheduledDeductionEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a scheduled deduction estimate code in its canonical SDE- form.</summary>
    public static string NormalizeScheduledDeductionEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SDE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SDE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
