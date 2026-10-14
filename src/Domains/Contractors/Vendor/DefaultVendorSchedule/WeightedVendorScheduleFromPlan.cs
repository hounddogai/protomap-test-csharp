namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Functions of weighted vendor schedule from plan.</summary>
public static class WeightedVendorScheduleFromPlan
{
    public static Dictionary<string, double> SummarizeFallbackTimesheetPlansForSchedule(List<FallbackTimesheetPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<WeightedVendorSchedule> DeriveWeightedVendorSchedulesFromPlan(List<FallbackTimesheetPlan> items)
    {
        var outValue = new List<WeightedVendorSchedule>();
        foreach (var item in items)
        {
            var derived = WeightedVendorSchedule.NewWeightedVendorSchedule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateWeightedVendorSchedule(WeightedVendorSchedule item)
    {
        return FallbackTimesheetPlanPolicy.NormalizeFallbackTimesheetPlanCode(item.Code);
    }
}
