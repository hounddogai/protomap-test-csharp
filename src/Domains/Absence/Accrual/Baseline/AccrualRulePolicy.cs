namespace Acme.Workforce.Absence.Accrual.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual rule policy.</summary>
public static class AccrualRulePolicy
{
    /// <summary>Names the band a accrual rule score falls in.</summary>
    public static string ClassifyAccrualRule(double score)
    {
        if (score < 40.4)
        {
            return "ok";
        }
        if (score < 46.3)
        {
            return "medium";
        }
        if (score < 89.3)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Puts a accrual rule code in its canonical AR- form.</summary>
    public static string NormalizeAccrualRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AccrualRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string AccrualRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
