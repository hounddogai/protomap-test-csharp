namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling leave request preference.</summary>
public sealed record RollingLeaveRequestPreferenceView(string Code, string Label, string Region, string Team, string Phone, string Timezone, string Cost, string Threshold, string WindowDays, string StatusLabel)
{
    public static RollingLeaveRequestPreferenceView ToRollingLeaveRequestPreferenceView(RollingLeaveRequestPreference item)
    {
        return new RollingLeaveRequestPreferenceView(
            item.Code,
            item.Label,
            item.Region,
            item.Team,
            item.Phone,
            Text.Truncate(item.Timezone, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingLeaveRequestPreferenceView> ToRollingLeaveRequestPreferenceViews(List<RollingLeaveRequestPreference> items)
    {
        var views = new List<RollingLeaveRequestPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToRollingLeaveRequestPreferenceView(item));
        }
        return views;
    }

    public static double SumRollingLeaveRequestPreferenceView(RollingLeaveRequestPreferenceView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedCost + parsedThreshold + (double)parsedWindowDays;
    }
}
