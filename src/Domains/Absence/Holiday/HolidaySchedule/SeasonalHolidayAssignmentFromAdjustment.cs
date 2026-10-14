namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Functions of seasonal holiday assignment from adjustment.</summary>
public static class SeasonalHolidayAssignmentFromAdjustment
{
    public static Dictionary<string, double> SummarizeFallbackLeaveRequestAdjustmentsForAssignment(List<FallbackLeaveRequestAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<SeasonalHolidayAssignment> DeriveSeasonalHolidayAssignmentsFromAdjustment(List<FallbackLeaveRequestAdjustment> items)
    {
        var outValue = new List<SeasonalHolidayAssignment>();
        foreach (var item in items)
        {
            var derived = SeasonalHolidayAssignment.NewSeasonalHolidayAssignment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectSeasonalHolidayAssignment(SeasonalHolidayAssignment item)
    {
        return LeaveRequestWindowPolicy.NormalizeLeaveRequestWindowCode(item.Code);
    }
}
