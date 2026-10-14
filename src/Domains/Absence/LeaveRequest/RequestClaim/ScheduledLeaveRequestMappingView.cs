namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled leave request mapping.</summary>
public sealed record ScheduledLeaveRequestMappingView(string Code, string TitleText, string Priority, string SegmentText, string ExpiresAt, string Threshold, string StatusLabel)
{
    public static ScheduledLeaveRequestMappingView ToScheduledLeaveRequestMappingView(ScheduledLeaveRequestMapping item)
    {
        return new ScheduledLeaveRequestMappingView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledLeaveRequestMappingView> ToScheduledLeaveRequestMappingViews(List<ScheduledLeaveRequestMapping> items)
    {
        var views = new List<ScheduledLeaveRequestMappingView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledLeaveRequestMappingView(item));
        }
        return views;
    }

    public static double SumScheduledLeaveRequestMappingView(ScheduledLeaveRequestMappingView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedExpiresAt + parsedThreshold;
    }
}
