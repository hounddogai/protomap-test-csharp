namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>Functions of merit cycle threshold from adjustment.</summary>
public static class MeritCycleThresholdFromAdjustment
{
    public static Dictionary<string, double> SummarizeEquityGrantAdjustmentsForThreshold(List<EquityGrantAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<MeritCycleThreshold> DeriveMeritCycleThresholdsFromAdjustment(List<EquityGrantAdjustment> items)
    {
        var outValue = new List<MeritCycleThreshold>();
        foreach (var item in items)
        {
            var derived = MeritCycleThreshold.NewMeritCycleThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitMeritCycleThreshold(MeritCycleThreshold item)
    {
        return EquityGrantRequestPolicy.EquityGrantRequestWeight(2, true);
    }
}
