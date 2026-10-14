namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive access badge checkpoint.</summary>
public sealed record AdaptiveAccessBadgeCheckpointView(string Code, string NameText, string Minimum, string Notes, string Phone, string Discount, string Archived, string Budget, string Ceiling, string Summary, string StatusLabel)
{
    public static AdaptiveAccessBadgeCheckpointView ToAdaptiveAccessBadgeCheckpointView(AdaptiveAccessBadgeCheckpoint item)
    {
        return new AdaptiveAccessBadgeCheckpointView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            Text.Truncate(item.Phone, 80),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveAccessBadgeCheckpointView> ToAdaptiveAccessBadgeCheckpointViews(List<AdaptiveAccessBadgeCheckpoint> items)
    {
        var views = new List<AdaptiveAccessBadgeCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveAccessBadgeCheckpointView(item));
        }
        return views;
    }

    public static double SumAdaptiveAccessBadgeCheckpointView(AdaptiveAccessBadgeCheckpointView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedDiscount + parsedBudget;
    }
}
