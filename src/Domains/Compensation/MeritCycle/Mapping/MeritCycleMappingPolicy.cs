namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of merit cycle mapping policy.</summary>
public static class MeritCycleMappingPolicy
{
    public static List<string> MergeMeritCycleMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string MeritCycleMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a merit cycle mapping code in its canonical MCM- form.</summary>
    public static string NormalizeMeritCycleMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a merit cycle mapping score falls in.</summary>
    public static string ClassifyMeritCycleMapping(double score)
    {
        if (score < 17.1)
        {
            return "high";
        }
        if (score < 21.2)
        {
            return "low";
        }
        if (score < 69.2)
        {
            return "minimal";
        }
        return "ok";
    }
}
