namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Functions of scheduled accrual override from metric.</summary>
public static class ScheduledAccrualOverrideFromMetric
{
    public static Dictionary<string, double> SummarizeBatchedAccrualMetricsForOverride(List<BatchedAccrualMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<ScheduledAccrualOverride> DeriveScheduledAccrualOverridesFromMetric(List<BatchedAccrualMetric> items)
    {
        var outValue = new List<ScheduledAccrualOverride>();
        foreach (var item in items)
        {
            var derived = ScheduledAccrualOverride.NewScheduledAccrualOverride(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateScheduledAccrualOverride(ScheduledAccrualOverride item)
    {
        return AccrualCheckpointPolicy.NormalizeAccrualCheckpointCode(item.Code);
    }
}
