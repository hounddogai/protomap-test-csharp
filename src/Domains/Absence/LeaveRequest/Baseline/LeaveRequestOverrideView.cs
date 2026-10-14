namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using System.Globalization;

/// <summary>How screens and exports show a leave request override.</summary>
public sealed record LeaveRequestOverrideView(string Code, string Label, string GraceDays, string Channel, string PriorityText, string Grade, string ChecksumText, string Reviewer, string StatusLabel)
{
    public static LeaveRequestOverrideView ToLeaveRequestOverrideView(LeaveRequestOverride item)
    {
        return new LeaveRequestOverrideView(
            item.Code,
            item.Label,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Checksum,
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestOverrideView> ToLeaveRequestOverrideViews(List<LeaveRequestOverride> items)
    {
        var views = new List<LeaveRequestOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestOverrideView(item));
        }
        return views;
    }

    public static double SumLeaveRequestOverrideView(LeaveRequestOverrideView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedPriority;
    }
}
