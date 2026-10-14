namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling access badge profile.</summary>
public sealed record RollingAccessBadgeProfileView(string Code, string Label, string WindowDays, string EmployeeIdText, string CreatedAt, string Capacity, string BadgeText, string PhoneText, string Revision, string StatusLabel)
{
    public static RollingAccessBadgeProfileView ToRollingAccessBadgeProfileView(RollingAccessBadgeProfile item)
    {
        return new RollingAccessBadgeProfileView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.Phone, 40),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingAccessBadgeProfileView> ToRollingAccessBadgeProfileViews(List<RollingAccessBadgeProfile> items)
    {
        var views = new List<RollingAccessBadgeProfileView>();
        foreach (var item in items)
        {
            views.Add(ToRollingAccessBadgeProfileView(item));
        }
        return views;
    }

    public static double SumRollingAccessBadgeProfileView(RollingAccessBadgeProfileView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedCreatedAt + (double)parsedCapacity;
    }
}
