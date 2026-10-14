namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Functions of strict offboarding override from plan.</summary>
public static class StrictOffboardingOverrideFromPlan
{
    public static Dictionary<string, double> SummarizePrimaryTimesheetPlansForOverride(List<PrimaryTimesheetPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<StrictOffboardingOverride> DeriveStrictOffboardingOverridesFromPlan(List<PrimaryTimesheetPlan> items)
    {
        var outValue = new List<StrictOffboardingOverride>();
        foreach (var item in items)
        {
            var derived = StrictOffboardingOverride.NewStrictOffboardingOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleStrictOffboardingOverride(StrictOffboardingOverride item)
    {
        return TimesheetManifestPolicy.ClassifyTimesheetManifest(item.NetTotal());
    }
}
