namespace Acme.Workforce.Compensation.BonusPool.Rule;

/// <summary>Functions of tiered bonus pool quota policy.</summary>
public static class TieredBonusPoolQuotaPolicy
{
    /// <summary>Puts a tiered bonus pool quota code in its canonical TBPQ- form.</summary>
    public static string NormalizeTieredBonusPoolQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TBPQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TBPQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a tiered bonus pool quota score falls in.</summary>
    public static string ClassifyTieredBonusPoolQuota(double score)
    {
        if (score < 13.7)
        {
            return "medium";
        }
        if (score < 85.6)
        {
            return "high";
        }
        return "elevated";
    }

    public static int TieredBonusPoolQuotaSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 9722.0, in order.</summary>
    public static int TieredBonusPoolQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9722.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
