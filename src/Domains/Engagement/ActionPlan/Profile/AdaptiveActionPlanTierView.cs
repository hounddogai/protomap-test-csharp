namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using System.Globalization;

/// <summary>How screens and exports show a adaptive action plan tier.</summary>
public sealed record AdaptiveActionPlanTierView(string Code, string Name, string Description, string Limit, string OwnerId, string Approved, string MinimumText, string CreatedAt, string StatusLabel)
{
    public static AdaptiveActionPlanTierView ToAdaptiveActionPlanTierView(AdaptiveActionPlanTier item)
    {
        return new AdaptiveActionPlanTierView(
            item.Code,
            item.Name,
            item.Description,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Approved}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveActionPlanTierView> ToAdaptiveActionPlanTierViews(List<AdaptiveActionPlanTier> items)
    {
        var views = new List<AdaptiveActionPlanTierView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveActionPlanTierView(item));
        }
        return views;
    }

    public static double SumAdaptiveActionPlanTierView(AdaptiveActionPlanTierView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedMinimum + (double)parsedCreatedAt;
    }
}
