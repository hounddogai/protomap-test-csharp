namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive action plan milestone.</summary>
public sealed record AdaptiveActionPlanMilestoneView(string Code, string TitleText, string ArchivedText, string StartsAtText, string Source, string DurationMinutesText, string StatusLabel)
{
    public static AdaptiveActionPlanMilestoneView ToAdaptiveActionPlanMilestoneView(AdaptiveActionPlanMilestone item)
    {
        return new AdaptiveActionPlanMilestoneView(
            Text.Truncate(item.Code, 64),
            item.Title,
            $"{item.Archived}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveActionPlanMilestoneView> ToAdaptiveActionPlanMilestoneViews(List<AdaptiveActionPlanMilestone> items)
    {
        var views = new List<AdaptiveActionPlanMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveActionPlanMilestoneView(item));
        }
        return views;
    }

    public static double SumAdaptiveActionPlanMilestoneView(AdaptiveActionPlanMilestoneView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedDurationMinutes;
    }
}
