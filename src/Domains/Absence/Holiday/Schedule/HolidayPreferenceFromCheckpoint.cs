namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Absence.ReturnPlan.Bundle;

/// <summary>Functions of holiday preference from checkpoint.</summary>
public static class HolidayPreferenceFromCheckpoint
{
    public static Dictionary<string, double> SummarizeReturnPlanCheckpointsForPreference(List<ReturnPlanCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<HolidayPreference> DeriveHolidayPreferencesFromCheckpoint(List<ReturnPlanCheckpoint> items)
    {
        var outValue = new List<HolidayPreference>();
        foreach (var item in items)
        {
            var derived = HolidayPreference.NewHolidayPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeHolidayPreference(HolidayPreference item)
    {
        return ReturnPlanBundlePolicy.NormalizeReturnPlanBundleCode(item.Code);
    }
}
