namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scenario request policy.</summary>
public static class ScenarioRequestPolicy
{
    /// <summary>Names the band a scenario request score falls in.</summary>
    public static string ClassifyScenarioRequest(double score)
    {
        if (score < 34.9)
        {
            return "medium";
        }
        if (score < 44.8)
        {
            return "minimal";
        }
        if (score < 56.4)
        {
            return "low";
        }
        return "critical";
    }

    /// <summary>Puts a scenario request code in its canonical SR- form.</summary>
    public static string NormalizeScenarioRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeScenarioRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ScenarioRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
