namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Functions of visitor override from agreement.</summary>
public static class VisitorOverrideFromAgreement
{
    public static Dictionary<string, double> SummarizeFloorPlanAgreementsForOverride(List<FloorPlanAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<VisitorOverride> DeriveVisitorOverridesFromAgreement(List<FloorPlanAgreement> items)
    {
        var outValue = new List<VisitorOverride>();
        foreach (var item in items)
        {
            var derived = VisitorOverride.NewVisitorOverride(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectVisitorOverride(VisitorOverride item)
    {
        return LegacyFloorPlanRulePolicy.LegacyFloorPlanRuleBucket(item.WeightedExposure());
    }
}
