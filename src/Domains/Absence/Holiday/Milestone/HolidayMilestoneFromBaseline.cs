namespace Acme.Workforce.Absence.Holiday.Milestone;

using Acme.Workforce.Absence.Holiday.Manifest;

/// <summary>Functions of holiday milestone from baseline.</summary>
public static class HolidayMilestoneFromBaseline
{
    public static Dictionary<string, double> SummarizeHolidayBaselinesForMilestone(List<HolidayBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<HolidayMilestone> DeriveHolidayMilestonesFromBaseline(List<HolidayBaseline> items)
    {
        var outValue = new List<HolidayMilestone>();
        foreach (var item in items)
        {
            var derived = HolidayMilestone.NewHolidayMilestone(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyHolidayMilestone(HolidayMilestone item)
    {
        return HolidayManifestPolicy.HolidayManifestWeight(1, true);
    }
}
