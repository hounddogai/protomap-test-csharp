namespace Acme.Workforce.Workplace.Booking.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global booking rule policy.</summary>
public static class GlobalBookingRulePolicy
{
    /// <summary>Puts a global booking rule code in its canonical GBR- form.</summary>
    public static string NormalizeGlobalBookingRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GBR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GBR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a global booking rule score falls in.</summary>
    public static string ClassifyGlobalBookingRule(double score)
    {
        if (score < 34.0)
        {
            return "minimal";
        }
        if (score < 83.3)
        {
            return "critical";
        }
        return "elevated";
    }

    public static List<string> MergeGlobalBookingRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string GlobalBookingRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
