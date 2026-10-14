namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>Functions of manual policy preference from schedule.</summary>
public static class ManualPolicyPreferenceFromSchedule
{
    public static Dictionary<string, double> SummarizeLeaveRequestSchedulesForPreference(List<LeaveRequestSchedule> items)
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

    public static List<ManualPolicyPreference> DeriveManualPolicyPreferencesFromSchedule(List<LeaveRequestSchedule> items)
    {
        var outValue = new List<ManualPolicyPreference>();
        foreach (var item in items)
        {
            var derived = ManualPolicyPreference.NewManualPolicyPreference(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyManualPolicyPreference(ManualPolicyPreference item)
    {
        return LenientLeaveRequestForecastPolicy.ClassifyLenientLeaveRequestForecast(item.WeightedLoad());
    }
}
