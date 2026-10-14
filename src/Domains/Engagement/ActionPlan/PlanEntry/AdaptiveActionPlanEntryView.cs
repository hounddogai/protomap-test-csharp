namespace Acme.Workforce.Engagement.ActionPlan.PlanEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive action plan entry.</summary>
public sealed record AdaptiveActionPlanEntryView(string Code, string Label, string Rating, string CapacityText, string MinimumText, string Version, string Grade, string CostText, string StatusLabel)
{
    public static AdaptiveActionPlanEntryView ToAdaptiveActionPlanEntryView(AdaptiveActionPlanEntry item)
    {
        return new AdaptiveActionPlanEntryView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 80),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveActionPlanEntryView> ToAdaptiveActionPlanEntryViews(List<AdaptiveActionPlanEntry> items)
    {
        var views = new List<AdaptiveActionPlanEntryView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveActionPlanEntryView(item));
        }
        return views;
    }

    public static double SumAdaptiveActionPlanEntryView(AdaptiveActionPlanEntryView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedCapacity + parsedMinimum;
    }
}
