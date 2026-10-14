namespace Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Functions of scheduled deduction cycle policy.</summary>
public static class ScheduledDeductionCyclePolicy
{
    public static int ScheduledDeductionCycleSpan(int start, int end)
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

    /// <summary>Puts a scheduled deduction cycle code in its canonical SDC- form.</summary>
    public static string NormalizeScheduledDeductionCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SDC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SDC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
