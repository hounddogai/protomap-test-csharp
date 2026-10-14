namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling access badge tier.</summary>
public sealed record RollingAccessBadgeTierView(string CodeText, string Name, string UpdatedAt, string Description, string TenureMonths, string ContactEmail, string StartsAt, string Approved, string ThresholdText, string LocaleText, string Checksum, string StatusLabel)
{
    public static RollingAccessBadgeTierView ToRollingAccessBadgeTierView(RollingAccessBadgeTier item)
    {
        return new RollingAccessBadgeTierView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 120),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingAccessBadgeTierView> ToRollingAccessBadgeTierViews(List<RollingAccessBadgeTier> items)
    {
        var views = new List<RollingAccessBadgeTierView>();
        foreach (var item in items)
        {
            views.Add(ToRollingAccessBadgeTierView(item));
        }
        return views;
    }

    public static double SumRollingAccessBadgeTierView(RollingAccessBadgeTierView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedTenureMonths + (double)parsedStartsAt;
    }
}
