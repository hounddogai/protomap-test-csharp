namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Functions of rolling recognition baseline from rule.</summary>
public static class RollingRecognitionBaselineFromRule
{
    public static Dictionary<string, double> SummarizeAdaptiveTownHallRulesForBaseline(List<AdaptiveTownHallRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<RollingRecognitionBaseline> DeriveRollingRecognitionBaselinesFromRule(List<AdaptiveTownHallRule> items)
    {
        var outValue = new List<RollingRecognitionBaseline>();
        foreach (var item in items)
        {
            var derived = RollingRecognitionBaseline.NewRollingRecognitionBaseline(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectRollingRecognitionBaseline(RollingRecognitionBaseline item)
    {
        return TownHallBudgetPolicy.NormalizeTownHallBudgetCode(item.Code);
    }
}
