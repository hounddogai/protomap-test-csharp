namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Functions of holiday metric from group.</summary>
public static class HolidayMetricFromGroup
{
    public static Dictionary<string, double> SummarizeLenientBalanceGroupsForMetric(List<LenientBalanceGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<HolidayMetric> DeriveHolidayMetricsFromGroup(List<LenientBalanceGroup> items)
    {
        var outValue = new List<HolidayMetric>();
        foreach (var item in items)
        {
            var derived = HolidayMetric.NewHolidayMetric(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeHolidayMetric(HolidayMetric item)
    {
        return RollingBalanceGroupPolicy.RollingBalanceGroupWeight(8, true);
    }
}
