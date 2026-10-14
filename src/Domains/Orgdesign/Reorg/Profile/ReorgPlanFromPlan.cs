namespace Acme.Workforce.Orgdesign.Reorg.Profile;

using Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>Functions of reorg plan from plan.</summary>
public static class ReorgPlanFromPlan
{
    public static Dictionary<string, double> SummarizePositionPlansForPlan(List<PositionPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<ReorgPlan> DeriveReorgPlansFromPlan(List<PositionPlan> items)
    {
        var outValue = new List<ReorgPlan>();
        foreach (var item in items)
        {
            var derived = ReorgPlan.NewReorgPlan(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileReorgPlan(ReorgPlan item)
    {
        return PositionPlanPolicy.ClassifyPositionPlan(item.AdjustedVolume());
    }
}
