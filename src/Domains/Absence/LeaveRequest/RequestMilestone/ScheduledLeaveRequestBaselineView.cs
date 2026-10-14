namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled leave request baseline.</summary>
public sealed record ScheduledLeaveRequestBaselineView(string Code, string Title, string Grade, string CeilingText, string Region, string FteText, string Location, string UpdatedAt, string VersionText, string Ratio, string StatusLabel)
{
    public static ScheduledLeaveRequestBaselineView ToScheduledLeaveRequestBaselineView(ScheduledLeaveRequestBaseline item)
    {
        return new ScheduledLeaveRequestBaselineView(
            item.Code,
            item.Title,
            item.Grade,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledLeaveRequestBaselineView> ToScheduledLeaveRequestBaselineViews(List<ScheduledLeaveRequestBaseline> items)
    {
        var views = new List<ScheduledLeaveRequestBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledLeaveRequestBaselineView(item));
        }
        return views;
    }

    public static double SumScheduledLeaveRequestBaselineView(ScheduledLeaveRequestBaselineView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedFte + (double)parsedUpdatedAt;
    }
}
