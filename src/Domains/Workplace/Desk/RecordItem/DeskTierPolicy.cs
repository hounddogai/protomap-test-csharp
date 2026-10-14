namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk tier policy.</summary>
public static class DeskTierPolicy
{
    /// <summary>Names the band a desk tier score falls in.</summary>
    public static string ClassifyDeskTier(double score)
    {
        if (score < 51.4)
        {
            return "medium";
        }
        if (score < 52.7)
        {
            return "low";
        }
        if (score < 97.9)
        {
            return "critical";
        }
        return "high";
    }

    public static double DeskTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.01;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string DeskTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a desk tier code in its canonical DT- form.</summary>
    public static string NormalizeDeskTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
