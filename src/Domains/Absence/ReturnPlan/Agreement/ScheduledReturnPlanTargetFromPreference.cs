namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Functions of scheduled return plan target from preference.</summary>
public static class ScheduledReturnPlanTargetFromPreference
{
    public static Dictionary<string, double> SummarizeHolidayPreferencesForTarget(List<HolidayPreference> items)
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

    public static List<ScheduledReturnPlanTarget> DeriveScheduledReturnPlanTargetsFromPreference(List<HolidayPreference> items)
    {
        var outValue = new List<ScheduledReturnPlanTarget>();
        foreach (var item in items)
        {
            var derived = ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectScheduledReturnPlanTarget(ScheduledReturnPlanTarget item)
    {
        return ManualHolidayVariancePolicy.ManualHolidayVarianceWeight(7, true);
    }
}
