namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Functions of merit cycle override from manifest.</summary>
public static class MeritCycleOverrideFromManifest
{
    public static Dictionary<string, double> SummarizeAdjustmentManifestsForOverride(List<AdjustmentManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<MeritCycleOverride> DeriveMeritCycleOverridesFromManifest(List<AdjustmentManifest> items)
    {
        var outValue = new List<MeritCycleOverride>();
        foreach (var item in items)
        {
            var derived = MeritCycleOverride.NewMeritCycleOverride(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyMeritCycleOverride(MeritCycleOverride item)
    {
        return GlobalAdjustmentRulePolicy.ClassifyGlobalAdjustmentRule(item.MeasuredValue());
    }
}
