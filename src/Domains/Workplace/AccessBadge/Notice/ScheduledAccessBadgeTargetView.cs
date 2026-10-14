namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled access badge target.</summary>
public sealed record ScheduledAccessBadgeTargetView(string Code, string Name, string TenureMonthsText, string OwnerId, string Threshold, string Approved, string StatusLabel)
{
    public static ScheduledAccessBadgeTargetView ToScheduledAccessBadgeTargetView(ScheduledAccessBadgeTarget item)
    {
        return new ScheduledAccessBadgeTargetView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAccessBadgeTargetView> ToScheduledAccessBadgeTargetViews(List<ScheduledAccessBadgeTarget> items)
    {
        var views = new List<ScheduledAccessBadgeTargetView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAccessBadgeTargetView(item));
        }
        return views;
    }

    public static double SumScheduledAccessBadgeTargetView(ScheduledAccessBadgeTargetView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedThreshold;
    }
}
