namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

/// <summary>Functions of succession plan window from adjustment.</summary>
public static class SuccessionPlanWindowFromAdjustment
{
    public static Dictionary<string, double> SummarizeRegionalSuccessionPlanAdjustmentsForWindow(List<RegionalSuccessionPlanAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<SuccessionPlanWindow> DeriveSuccessionPlanWindowsFromAdjustment(List<RegionalSuccessionPlanAdjustment> items)
    {
        var outValue = new List<SuccessionPlanWindow>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanWindow.NewSuccessionPlanWindow(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeSuccessionPlanWindow(SuccessionPlanWindow item)
    {
        return RegionalSuccessionPlanAdjustmentPolicy.RegionalSuccessionPlanAdjustmentBucket(item.WeightedVolume());
    }
}
