namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band plan policy.</summary>
public static class PayBandPlanPolicy
{
    public static int PayBandPlanSpan(int start, int end)
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
        return span / 86400;
    }

    public static string PayBandPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a pay band plan code in its canonical PBP- form.</summary>
    public static string NormalizePayBandPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a pay band plan score falls in.</summary>
    public static string ClassifyPayBandPlan(double score)
    {
        if (score < 4.2)
        {
            return "elevated";
        }
        if (score < 24.5)
        {
            return "medium";
        }
        if (score < 59.1)
        {
            return "watch";
        }
        return "critical";
    }
}
