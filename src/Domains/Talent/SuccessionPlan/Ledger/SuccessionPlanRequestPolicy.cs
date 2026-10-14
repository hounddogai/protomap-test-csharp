namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Functions of succession plan request policy.</summary>
public static class SuccessionPlanRequestPolicy
{
    public static int SuccessionPlanRequestSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 22747.0, in order.</summary>
    public static int SuccessionPlanRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22747.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a succession plan request code in its canonical SPR- form.</summary>
    public static string NormalizeSuccessionPlanRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
