namespace Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Functions of accrual baseline policy.</summary>
public static class AccrualBaselinePolicy
{
    public static int AccrualBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 59177.0, in order.</summary>
    public static int AccrualBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 59177.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a accrual baseline code in its canonical AB- form.</summary>
    public static string NormalizeAccrualBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
