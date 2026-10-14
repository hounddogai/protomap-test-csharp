namespace Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Functions of manual accrual policy policy.</summary>
public static class ManualAccrualPolicyPolicy
{
    /// <summary>Puts a manual accrual policy code in its canonical MAP- form.</summary>
    public static string NormalizeManualAccrualPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MAP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MAP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ManualAccrualPolicySpan(int start, int end)
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
        return span / 86400;
    }
}
