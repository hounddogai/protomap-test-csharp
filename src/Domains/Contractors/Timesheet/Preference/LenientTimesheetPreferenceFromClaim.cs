namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>Functions of lenient timesheet preference from claim.</summary>
public static class LenientTimesheetPreferenceFromClaim
{
    public static Dictionary<string, double> SummarizeTimesheetClaimsForPreference(List<TimesheetClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<LenientTimesheetPreference> DeriveLenientTimesheetPreferencesFromClaim(List<TimesheetClaim> items)
    {
        var outValue = new List<LenientTimesheetPreference>();
        foreach (var item in items)
        {
            var derived = LenientTimesheetPreference.NewLenientTimesheetPreference(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceLenientTimesheetPreference(LenientTimesheetPreference item)
    {
        return TimesheetBundlePolicy.TimesheetBundleBucket(item.WeightedVolume());
    }
}
