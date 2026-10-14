namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>Functions of default holiday milestone from template.</summary>
public static class DefaultHolidayMilestoneFromTemplate
{
    public static Dictionary<string, double> SummarizeStrictAccrualTemplatesForMilestone(List<StrictAccrualTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<DefaultHolidayMilestone> DeriveDefaultHolidayMilestonesFromTemplate(List<StrictAccrualTemplate> items)
    {
        var outValue = new List<DefaultHolidayMilestone>();
        foreach (var item in items)
        {
            var derived = DefaultHolidayMilestone.NewDefaultHolidayMilestone(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectDefaultHolidayMilestone(DefaultHolidayMilestone item)
    {
        return AdaptiveAccrualMetricPolicy.AdaptiveAccrualMetricBucket(item.NetVolume());
    }
}
