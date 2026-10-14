namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band adjustment policy.</summary>
public static class PayBandAdjustmentPolicy
{
    /// <summary>Names the band a pay band adjustment score falls in.</summary>
    public static string ClassifyPayBandAdjustment(double score)
    {
        if (score < 6.3)
        {
            return "low";
        }
        if (score < 71.7)
        {
            return "watch";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 70157.0, in order.</summary>
    public static int PayBandAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70157.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PayBandAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a pay band adjustment code in its canonical PBA- form.</summary>
    public static string NormalizePayBandAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
