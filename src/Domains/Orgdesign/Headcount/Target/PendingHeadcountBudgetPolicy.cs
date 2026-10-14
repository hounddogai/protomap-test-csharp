namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending headcount budget policy.</summary>
public static class PendingHeadcountBudgetPolicy
{
    public static double PendingHeadcountBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a pending headcount budget score falls in.</summary>
    public static string ClassifyPendingHeadcountBudget(double score)
    {
        if (score < 27.8)
        {
            return "ok";
        }
        if (score < 31.7)
        {
            return "medium";
        }
        if (score < 60.5)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Puts a pending headcount budget code in its canonical PHB- form.</summary>
    public static string NormalizePendingHeadcountBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PHB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PHB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PendingHeadcountBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
