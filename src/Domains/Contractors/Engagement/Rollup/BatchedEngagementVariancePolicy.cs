namespace Acme.Workforce.Contractors.Engagement.Rollup;

/// <summary>Functions of batched engagement variance policy.</summary>
public static class BatchedEngagementVariancePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 29306.0, in order.</summary>
    public static int BatchedEngagementVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29306.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a batched engagement variance score falls in.</summary>
    public static string ClassifyBatchedEngagementVariance(double score)
    {
        if (score < 58.0)
        {
            return "high";
        }
        if (score < 95.6)
        {
            return "elevated";
        }
        return "low";
    }

    public static int BatchedEngagementVarianceSpan(int start, int end)
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

    /// <summary>Puts a batched engagement variance code in its canonical BEV- form.</summary>
    public static string NormalizeBatchedEngagementVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BEV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BEV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
