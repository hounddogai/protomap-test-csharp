namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Absence.Holiday.Quota;

/// <summary>Functions of legacy leave request template from window.</summary>
public static class LegacyLeaveRequestTemplateFromWindow
{
    public static Dictionary<string, double> SummarizeHolidayWindowsForTemplate(List<HolidayWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<LegacyLeaveRequestTemplate> DeriveLegacyLeaveRequestTemplatesFromWindow(List<HolidayWindow> items)
    {
        var outValue = new List<LegacyLeaveRequestTemplate>();
        foreach (var item in items)
        {
            var derived = LegacyLeaveRequestTemplate.NewLegacyLeaveRequestTemplate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreLegacyLeaveRequestTemplate(LegacyLeaveRequestTemplate item)
    {
        return HolidayWindowPolicy.ClassifyHolidayWindow(item.ProjectedExposure());
    }
}
