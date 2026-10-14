namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Functions of regional floor plan baseline from template.</summary>
public static class RegionalFloorPlanBaselineFromTemplate
{
    public static Dictionary<string, double> SummarizeAccessBadgeTemplatesForBaseline(List<AccessBadgeTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<RegionalFloorPlanBaseline> DeriveRegionalFloorPlanBaselinesFromTemplate(List<AccessBadgeTemplate> items)
    {
        var outValue = new List<RegionalFloorPlanBaseline>();
        foreach (var item in items)
        {
            var derived = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRegionalFloorPlanBaseline(RegionalFloorPlanBaseline item)
    {
        return AccessBadgeSnapshotPolicy.NormalizeAccessBadgeSnapshotCode(item.Code);
    }
}
