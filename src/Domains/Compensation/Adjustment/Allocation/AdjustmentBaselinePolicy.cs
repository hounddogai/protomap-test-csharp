namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment baseline policy.</summary>
public static class AdjustmentBaselinePolicy
{
    public static double AdjustmentBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a adjustment baseline score falls in.</summary>
    public static string ClassifyAdjustmentBaseline(double score)
    {
        if (score < 18.1)
        {
            return "low";
        }
        if (score < 45.5)
        {
            return "elevated";
        }
        if (score < 99.8)
        {
            return "medium";
        }
        return "ok";
    }

    /// <summary>Puts a adjustment baseline code in its canonical AB- form.</summary>
    public static string NormalizeAdjustmentBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AdjustmentBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
