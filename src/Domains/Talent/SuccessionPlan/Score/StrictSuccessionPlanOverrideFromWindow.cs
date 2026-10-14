namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Functions of strict succession plan override from window.</summary>
public static class StrictSuccessionPlanOverrideFromWindow
{
    public static Dictionary<string, double> SummarizeCappedNominationWindowsForOverride(List<CappedNominationWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<StrictSuccessionPlanOverride> DeriveStrictSuccessionPlanOverridesFromWindow(List<CappedNominationWindow> items)
    {
        var outValue = new List<StrictSuccessionPlanOverride>();
        foreach (var item in items)
        {
            var derived = StrictSuccessionPlanOverride.NewStrictSuccessionPlanOverride(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitStrictSuccessionPlanOverride(StrictSuccessionPlanOverride item)
    {
        return NominationSlotPolicy.ClassifyNominationSlot(item.ProjectedExposure());
    }
}
