namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled scenario manifest policy.</summary>
public static class ScheduledScenarioManifestPolicy
{
    public static int ScheduledScenarioManifestSpan(int start, int end)
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

    public static double ScheduledScenarioManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.93;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeScheduledScenarioManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a scheduled scenario manifest code in its canonical SSM- form.</summary>
    public static string NormalizeScheduledScenarioManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
