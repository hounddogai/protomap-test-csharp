namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request notice.</summary>
public sealed record LeaveRequestNoticeView(string Code, string Name, string WeightText, string LeaveDays, string Attempts, string Description, string Checksum, string StatusLabel)
{
    public static LeaveRequestNoticeView ToLeaveRequestNoticeView(LeaveRequestNotice item)
    {
        return new LeaveRequestNoticeView(
            item.Code,
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.Checksum, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestNoticeView> ToLeaveRequestNoticeViews(List<LeaveRequestNotice> items)
    {
        var views = new List<LeaveRequestNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestNoticeView(item));
        }
        return views;
    }

    public static double SumLeaveRequestNoticeView(LeaveRequestNoticeView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedLeaveDays + (double)parsedAttempts;
    }
}
