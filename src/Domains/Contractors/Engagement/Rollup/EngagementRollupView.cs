namespace Acme.Workforce.Contractors.Engagement.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement rollup.</summary>
public sealed record EngagementRollupView(string CodeText, string Label, string Skill, string TimezoneText, string FloorValue, string HoursText, string PhoneText, string Rating, string UpdatedAt, string StatusLabel)
{
    public static EngagementRollupView ToEngagementRollupView(EngagementRollup item)
    {
        return new EngagementRollupView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Skill,
            item.Timezone,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementRollupView> ToEngagementRollupViews(List<EngagementRollup> items)
    {
        var views = new List<EngagementRollupView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementRollupView(item));
        }
        return views;
    }

    public static double SumEngagementRollupView(EngagementRollupView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedHours + parsedRating;
    }
}
