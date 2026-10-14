namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Functions of default pool milestone from cycle.</summary>
public static class DefaultPoolMilestoneFromCycle
{
    public static Dictionary<string, double> SummarizePoolCyclesForMilestone(List<PoolCycle> items)
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

    public static List<DefaultPoolMilestone> DeriveDefaultPoolMilestonesFromCycle(List<PoolCycle> items)
    {
        var outValue = new List<DefaultPoolMilestone>();
        foreach (var item in items)
        {
            var derived = DefaultPoolMilestone.NewDefaultPoolMilestone(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignDefaultPoolMilestone(DefaultPoolMilestone item)
    {
        return ScheduledPoolWindowPolicy.ScheduledPoolWindowWeight(5, true);
    }
}
