namespace Acme.Workforce.Compensation.Adjustment.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global adjustment summary.</summary>
public sealed record GlobalAdjustmentSummaryView(string Code, string TitleText, string Summary, string Approved, string LocationText, string Attempts, string EndsAt, string RegionText, string Channel, string StatusLabel)
{
    public static GlobalAdjustmentSummaryView ToGlobalAdjustmentSummaryView(GlobalAdjustmentSummary item)
    {
        return new GlobalAdjustmentSummaryView(
            item.Code,
            item.Title,
            item.Summary,
            $"{item.Approved}",
            Text.Truncate(item.Location, 80),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAdjustmentSummaryView> ToGlobalAdjustmentSummaryViews(List<GlobalAdjustmentSummary> items)
    {
        var views = new List<GlobalAdjustmentSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAdjustmentSummaryView(item));
        }
        return views;
    }

    public static double SumGlobalAdjustmentSummaryView(GlobalAdjustmentSummaryView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedEndsAt;
    }
}
