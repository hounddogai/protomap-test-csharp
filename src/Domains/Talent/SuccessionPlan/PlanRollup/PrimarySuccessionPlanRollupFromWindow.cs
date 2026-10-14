namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Functions of primary succession plan rollup from window.</summary>
public static class PrimarySuccessionPlanRollupFromWindow
{
    public static Dictionary<string, double> SummarizeSuccessionPlanWindowsForRollup(List<SuccessionPlanWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<PrimarySuccessionPlanRollup> DerivePrimarySuccessionPlanRollupsFromWindow(List<SuccessionPlanWindow> items)
    {
        var outValue = new List<PrimarySuccessionPlanRollup>();
        foreach (var item in items)
        {
            var derived = PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePrimarySuccessionPlanRollup(PrimarySuccessionPlanRollup item)
    {
        return LenientSuccessionPlanRequestPolicy.LenientSuccessionPlanRequestBucket(item.AdjustedExposure());
    }
}
