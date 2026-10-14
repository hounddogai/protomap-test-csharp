namespace Acme.Workforce.Absence.Holiday.Budget;

using Acme.Workforce.Absence.Policy.Baseline;

/// <summary>Functions of holiday entry from baseline.</summary>
public static class HolidayEntryFromBaseline
{
    public static Dictionary<string, double> SummarizePolicyBaselinesForEntry(List<PolicyBaseline> items)
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

    public static List<HolidayEntry> DeriveHolidayEntriesFromBaseline(List<PolicyBaseline> items)
    {
        var outValue = new List<HolidayEntry>();
        foreach (var item in items)
        {
            var derived = HolidayEntry.NewHolidayEntry(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanHolidayEntry(HolidayEntry item)
    {
        return PolicyBaselinePolicy.PolicyBaselineWeight(5, true);
    }
}
