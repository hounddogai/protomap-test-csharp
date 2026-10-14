namespace Acme.Workforce.Absence.LeaveRequest.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual leave request score.</summary>
public sealed record ManualLeaveRequestScoreView(string CodeText, string Name, string JobCodeText, string Category, string Skill, string LeaveDaysText, string WindowDays, string UnitText, string Ratio, string StatusLabel)
{
    public static ManualLeaveRequestScoreView ToManualLeaveRequestScoreView(ManualLeaveRequestScore item)
    {
        return new ManualLeaveRequestScoreView(
            item.Code,
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.JobCode, 80),
            item.Category,
            item.Skill,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualLeaveRequestScoreView> ToManualLeaveRequestScoreViews(List<ManualLeaveRequestScore> items)
    {
        var views = new List<ManualLeaveRequestScoreView>();
        foreach (var item in items)
        {
            views.Add(ToManualLeaveRequestScoreView(item));
        }
        return views;
    }

    public static double SumManualLeaveRequestScoreView(ManualLeaveRequestScoreView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedWindowDays + parsedRatio;
    }
}
