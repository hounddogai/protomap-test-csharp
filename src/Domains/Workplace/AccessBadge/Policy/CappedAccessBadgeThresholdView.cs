namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped access badge threshold.</summary>
public sealed record CappedAccessBadgeThresholdView(string CodeText, string Title, string Reviewer, string Grade, string PriorityText, string StatusLabel)
{
    public static CappedAccessBadgeThresholdView ToCappedAccessBadgeThresholdView(CappedAccessBadgeThreshold item)
    {
        return new CappedAccessBadgeThresholdView(
            item.Code,
            item.Title,
            Text.Truncate(item.Reviewer, 120),
            Text.Truncate(item.Grade, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAccessBadgeThresholdView> ToCappedAccessBadgeThresholdViews(List<CappedAccessBadgeThreshold> items)
    {
        var views = new List<CappedAccessBadgeThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAccessBadgeThresholdView(item));
        }
        return views;
    }

    public static double SumCappedAccessBadgeThresholdView(CappedAccessBadgeThresholdView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedPriority;
    }
}
