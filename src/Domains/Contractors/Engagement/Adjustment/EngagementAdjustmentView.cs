namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement adjustment.</summary>
public sealed record EngagementAdjustmentView(string Code, string Title, string ExternalRef, string StartsAt, string Category, string Description, string Maximum, string TenureMonthsText, string ExpiresAt, string StatusLabel)
{
    public static EngagementAdjustmentView ToEngagementAdjustmentView(EngagementAdjustment item)
    {
        return new EngagementAdjustmentView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.ExternalRef,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.Description, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementAdjustmentView> ToEngagementAdjustmentViews(List<EngagementAdjustment> items)
    {
        var views = new List<EngagementAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementAdjustmentView(item));
        }
        return views;
    }

    public static double SumEngagementAdjustmentView(EngagementAdjustmentView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedMaximum + (double)parsedTenureMonths;
    }
}
