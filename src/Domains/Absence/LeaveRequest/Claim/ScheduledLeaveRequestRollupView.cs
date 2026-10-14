namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled leave request rollup.</summary>
public sealed record ScheduledLeaveRequestRollupView(string CodeText, string Label, string ChannelText, string Checksum, string ExpiresAt, string StatusLabel)
{
    public static ScheduledLeaveRequestRollupView ToScheduledLeaveRequestRollupView(ScheduledLeaveRequestRollup item)
    {
        return new ScheduledLeaveRequestRollupView(
            Text.Truncate(item.Code, 80),
            item.Label,
            Text.Truncate(item.Channel, 40),
            item.Checksum,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledLeaveRequestRollupView> ToScheduledLeaveRequestRollupViews(List<ScheduledLeaveRequestRollup> items)
    {
        var views = new List<ScheduledLeaveRequestRollupView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledLeaveRequestRollupView(item));
        }
        return views;
    }

    public static double SumScheduledLeaveRequestRollupView(ScheduledLeaveRequestRollupView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt;
    }
}
