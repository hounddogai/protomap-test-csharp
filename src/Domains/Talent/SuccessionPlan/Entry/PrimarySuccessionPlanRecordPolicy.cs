namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>Functions of primary succession plan record policy.</summary>
public static class PrimarySuccessionPlanRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 50642.0, in order.</summary>
    public static int PrimarySuccessionPlanRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50642.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a primary succession plan record code in its canonical PSPR- form.</summary>
    public static string NormalizePrimarySuccessionPlanRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PrimarySuccessionPlanRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
