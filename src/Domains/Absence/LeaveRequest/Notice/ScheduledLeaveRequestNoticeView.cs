namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled leave request notice.</summary>
public sealed record ScheduledLeaveRequestNoticeView(string Code, string NameText, string ChannelText, string Pinned, string ExpiresAt, string StatusLabel)
{
    public static ScheduledLeaveRequestNoticeView ToScheduledLeaveRequestNoticeView(ScheduledLeaveRequestNotice item)
    {
        return new ScheduledLeaveRequestNoticeView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.Channel, 40),
            $"{item.Pinned}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledLeaveRequestNoticeView> ToScheduledLeaveRequestNoticeViews(List<ScheduledLeaveRequestNotice> items)
    {
        var views = new List<ScheduledLeaveRequestNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledLeaveRequestNoticeView(item));
        }
        return views;
    }

    public static double SumScheduledLeaveRequestNoticeView(ScheduledLeaveRequestNoticeView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
