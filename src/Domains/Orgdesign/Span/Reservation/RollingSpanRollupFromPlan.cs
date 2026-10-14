namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Functions of rolling span rollup from plan.</summary>
public static class RollingSpanRollupFromPlan
{
    public static Dictionary<string, double> SummarizePrimaryReorgPlansForRollup(List<PrimaryReorgPlan> items)
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

    public static List<RollingSpanRollup> DeriveRollingSpanRollupsFromPlan(List<PrimaryReorgPlan> items)
    {
        var outValue = new List<RollingSpanRollup>();
        foreach (var item in items)
        {
            var derived = RollingSpanRollup.NewRollingSpanRollup(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeRollingSpanRollup(RollingSpanRollup item)
    {
        return RollingReorgBundlePolicy.RollingReorgBundleWeight(7, true);
    }
}
