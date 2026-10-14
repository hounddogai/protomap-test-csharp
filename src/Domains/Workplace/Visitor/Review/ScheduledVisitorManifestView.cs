namespace Acme.Workforce.Workplace.Visitor.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled visitor manifest.</summary>
public sealed record ScheduledVisitorManifestView(string Code, string Title, string LeaveDays, string Channel, string Pinned, string PhoneText, string RatioText, string Grade, string CountryText, string UpdatedAtText, string Hours, string StatusLabel)
{
    public static ScheduledVisitorManifestView ToScheduledVisitorManifestView(ScheduledVisitorManifest item)
    {
        return new ScheduledVisitorManifestView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            $"{item.Pinned}",
            item.Phone,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Country,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledVisitorManifestView> ToScheduledVisitorManifestViews(List<ScheduledVisitorManifest> items)
    {
        var views = new List<ScheduledVisitorManifestView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledVisitorManifestView(item));
        }
        return views;
    }

    public static double SumScheduledVisitorManifestView(ScheduledVisitorManifestView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedRatio + (double)parsedUpdatedAt;
    }
}
