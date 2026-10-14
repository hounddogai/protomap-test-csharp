namespace Acme.Workforce.Workplace.AccessBadge.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default access badge manifest.</summary>
public sealed record DefaultAccessBadgeManifestView(string Code, string Label, string ContactEmailText, string Segment, string HoursText, string DurationMinutes, string Description, string EmployeeIdText, string Country, string StatusLabel)
{
    public static DefaultAccessBadgeManifestView ToDefaultAccessBadgeManifestView(DefaultAccessBadgeManifest item)
    {
        return new DefaultAccessBadgeManifestView(
            Text.Truncate(item.Code, 120),
            item.Label,
            Text.Truncate(item.ContactEmail, 120),
            item.Segment,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.EmployeeId,
            Text.Truncate(item.Country, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAccessBadgeManifestView> ToDefaultAccessBadgeManifestViews(List<DefaultAccessBadgeManifest> items)
    {
        var views = new List<DefaultAccessBadgeManifestView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAccessBadgeManifestView(item));
        }
        return views;
    }

    public static double SumDefaultAccessBadgeManifestView(DefaultAccessBadgeManifestView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedDurationMinutes;
    }
}
