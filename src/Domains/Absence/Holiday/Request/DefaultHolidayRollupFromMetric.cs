namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>Functions of default holiday rollup from metric.</summary>
public static class DefaultHolidayRollupFromMetric
{
    public static Dictionary<string, double> SummarizeReturnPlanMetricsForRollup(List<ReturnPlanMetric> items)
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

    public static List<DefaultHolidayRollup> DeriveDefaultHolidayRollupsFromMetric(List<ReturnPlanMetric> items)
    {
        var outValue = new List<DefaultHolidayRollup>();
        foreach (var item in items)
        {
            var derived = DefaultHolidayRollup.NewDefaultHolidayRollup(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateDefaultHolidayRollup(DefaultHolidayRollup item)
    {
        return StrictReturnPlanSlotPolicy.StrictReturnPlanSlotWeight(8, true);
    }
}
