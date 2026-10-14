namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched pay band threshold policy.</summary>
public static class BatchedPayBandThresholdPolicy
{
    public static string BatchedPayBandThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 15137.0, in order.</summary>
    public static int BatchedPayBandThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15137.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a batched pay band threshold code in its canonical BPBT- form.</summary>
    public static string NormalizeBatchedPayBandThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int BatchedPayBandThresholdSpan(int start, int end)
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
}
