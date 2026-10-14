namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of audit forecast policy.</summary>
public static class AuditForecastPolicy
{
    public static List<string> MergeAuditForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string AuditForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a audit forecast score falls in.</summary>
    public static string ClassifyAuditForecast(double score)
    {
        if (score < 47.9)
        {
            return "low";
        }
        if (score < 50.6)
        {
            return "elevated";
        }
        if (score < 78.2)
        {
            return "critical";
        }
        if (score < 80.5)
        {
            return "high";
        }
        return "minimal";
    }

    public static double AuditForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
