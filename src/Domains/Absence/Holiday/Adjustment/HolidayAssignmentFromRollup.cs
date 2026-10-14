namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Mapping;

/// <summary>Functions of holiday assignment from rollup.</summary>
public static class HolidayAssignmentFromRollup
{
    public static Dictionary<string, double> SummarizeHolidayRollupsForAssignment(List<HolidayRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<HolidayAssignment> DeriveHolidayAssignmentsFromRollup(List<HolidayRollup> items)
    {
        var outValue = new List<HolidayAssignment>();
        foreach (var item in items)
        {
            var derived = HolidayAssignment.NewHolidayAssignment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeHolidayAssignment(HolidayAssignment item)
    {
        return SeasonalHolidayClaimPolicy.SeasonalHolidayClaimWeight(8, true);
    }
}
