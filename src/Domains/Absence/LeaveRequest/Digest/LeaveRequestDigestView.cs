namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request digest.</summary>
public sealed record LeaveRequestDigestView(string Code, string NameText, string RegionText, string Currency, string Channel, string MarginText, string Rating, string ActiveText, string EndsAtText, string UnitText, string WindowDays, string StatusLabel)
{
    public static LeaveRequestDigestView ToLeaveRequestDigestView(LeaveRequestDigest item)
    {
        return new LeaveRequestDigestView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Region,
            Text.Truncate(item.Currency, 80),
            item.Channel,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestDigestView> ToLeaveRequestDigestViews(List<LeaveRequestDigest> items)
    {
        var views = new List<LeaveRequestDigestView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestDigestView(item));
        }
        return views;
    }

    public static double SumLeaveRequestDigestView(LeaveRequestDigestView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return parsedMargin + parsedRating + (double)parsedEndsAt;
    }
}
