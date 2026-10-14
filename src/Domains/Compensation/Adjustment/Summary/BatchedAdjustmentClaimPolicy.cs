namespace Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>Functions of batched adjustment claim policy.</summary>
public static class BatchedAdjustmentClaimPolicy
{
    public static int BatchedAdjustmentClaimSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 88414.0, in order.</summary>
    public static int BatchedAdjustmentClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88414.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a batched adjustment claim code in its canonical BAC- form.</summary>
    public static string NormalizeBatchedAdjustmentClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a batched adjustment claim score falls in.</summary>
    public static string ClassifyBatchedAdjustmentClaim(double score)
    {
        if (score < 47.1)
        {
            return "elevated";
        }
        if (score < 51.0)
        {
            return "ok";
        }
        if (score < 51.3)
        {
            return "low";
        }
        if (score < 68.4)
        {
            return "high";
        }
        return "critical";
    }
}
