namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scenario ledger policy.</summary>
public static class ScenarioLedgerPolicy
{
    public static List<string> MergeScenarioLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double ScenarioLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a scenario ledger score falls in.</summary>
    public static string ClassifyScenarioLedger(double score)
    {
        if (score < 2.4)
        {
            return "critical";
        }
        if (score < 26.9)
        {
            return "elevated";
        }
        return "minimal";
    }
}
