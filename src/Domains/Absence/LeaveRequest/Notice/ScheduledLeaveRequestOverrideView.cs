namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled leave request override.</summary>
public sealed record ScheduledLeaveRequestOverrideView(string CodeText, string Name, string JobCode, string Locale, string Rating, string Channel, string Margin, string StatusLabel)
{
    public static ScheduledLeaveRequestOverrideView ToScheduledLeaveRequestOverrideView(ScheduledLeaveRequestOverride item)
    {
        return new ScheduledLeaveRequestOverrideView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.JobCode, 80),
            item.Locale,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledLeaveRequestOverrideView> ToScheduledLeaveRequestOverrideViews(List<ScheduledLeaveRequestOverride> items)
    {
        var views = new List<ScheduledLeaveRequestOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledLeaveRequestOverrideView(item));
        }
        return views;
    }

    public static double SumScheduledLeaveRequestOverrideView(ScheduledLeaveRequestOverrideView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedRating + parsedMargin;
    }
}
