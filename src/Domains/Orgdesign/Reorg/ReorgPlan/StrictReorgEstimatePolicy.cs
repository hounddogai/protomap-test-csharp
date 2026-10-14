namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Functions of strict reorg estimate policy.</summary>
public static class StrictReorgEstimatePolicy
{
    /// <summary>Puts a strict reorg estimate code in its canonical SRE- form.</summary>
    public static string NormalizeStrictReorgEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 4549.0, in order.</summary>
    public static int StrictReorgEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4549.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
