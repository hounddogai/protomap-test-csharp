namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Functions of rolling reorg notice from assignment.</summary>
public static class RollingReorgNoticeFromAssignment
{
    public static Dictionary<string, double> SummarizeCostModelAssignmentsForNotice(List<CostModelAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<RollingReorgNotice> DeriveRollingReorgNoticesFromAssignment(List<CostModelAssignment> items)
    {
        var outValue = new List<RollingReorgNotice>();
        foreach (var item in items)
        {
            var derived = RollingReorgNotice.NewRollingReorgNotice(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeRollingReorgNotice(RollingReorgNotice item)
    {
        return CostModelBatchPolicy.CostModelBatchBucket(item.WeightedVolume());
    }
}
