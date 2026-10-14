namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending scenario template policy.</summary>
public static class PendingScenarioTemplatePolicy
{
    public static int PendingScenarioTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a pending scenario template score falls in.</summary>
    public static string ClassifyPendingScenarioTemplate(double score)
    {
        if (score < 28.4)
        {
            return "medium";
        }
        if (score < 43.8)
        {
            return "high";
        }
        if (score < 56.4)
        {
            return "critical";
        }
        return "minimal";
    }

    public static string PendingScenarioTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static List<string> MergePendingScenarioTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
