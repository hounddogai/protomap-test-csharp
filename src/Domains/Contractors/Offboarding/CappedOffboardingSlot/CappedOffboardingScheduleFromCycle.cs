namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Functions of capped offboarding schedule from cycle.</summary>
public static class CappedOffboardingScheduleFromCycle
{
    public static Dictionary<string, double> SummarizeWeightedRateCyclesForSchedule(List<WeightedRateCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<CappedOffboardingSchedule> DeriveCappedOffboardingSchedulesFromCycle(List<WeightedRateCycle> items)
    {
        var outValue = new List<CappedOffboardingSchedule>();
        foreach (var item in items)
        {
            var derived = CappedOffboardingSchedule.NewCappedOffboardingSchedule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeCappedOffboardingSchedule(CappedOffboardingSchedule item)
    {
        return WeightedRateCyclePolicy.WeightedRateCycleWeight(8, true);
    }
}
