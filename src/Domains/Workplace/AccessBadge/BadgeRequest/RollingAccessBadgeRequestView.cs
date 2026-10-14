namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling access badge request.</summary>
public sealed record RollingAccessBadgeRequestView(string Code, string Label, string StartsAt, string Summary, string Country, string EndsAt, string Ceiling, string Timezone, string MaximumText, string RateText, string StatusLabel)
{
    public static RollingAccessBadgeRequestView ToRollingAccessBadgeRequestView(RollingAccessBadgeRequest item)
    {
        return new RollingAccessBadgeRequestView(
            item.Code,
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 40),
            item.Country,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingAccessBadgeRequestView> ToRollingAccessBadgeRequestViews(List<RollingAccessBadgeRequest> items)
    {
        var views = new List<RollingAccessBadgeRequestView>();
        foreach (var item in items)
        {
            views.Add(ToRollingAccessBadgeRequestView(item));
        }
        return views;
    }

    public static double SumRollingAccessBadgeRequestView(RollingAccessBadgeRequestView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedEndsAt + parsedCeiling;
    }
}
