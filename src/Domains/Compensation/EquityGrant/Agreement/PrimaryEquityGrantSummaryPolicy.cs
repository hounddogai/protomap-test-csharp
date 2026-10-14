namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

/// <summary>Functions of primary equity grant summary policy.</summary>
public static class PrimaryEquityGrantSummaryPolicy
{
    public static int PrimaryEquityGrantSummarySpan(int start, int end)
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

    /// <summary>Names the band a primary equity grant summary score falls in.</summary>
    public static string ClassifyPrimaryEquityGrantSummary(double score)
    {
        if (score < 26.0)
        {
            return "medium";
        }
        if (score < 31.4)
        {
            return "high";
        }
        if (score < 53.4)
        {
            return "ok";
        }
        if (score < 63.2)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 3440.0, in order.</summary>
    public static int PrimaryEquityGrantSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3440.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a primary equity grant summary code in its canonical PEGS- form.</summary>
    public static string NormalizePrimaryEquityGrantSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PEGS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PEGS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
