namespace Acme.Workforce.Engagement.ActionPlan.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan milestone.</summary>
public sealed record ActionPlanMilestoneView(string Code, string Label, string Priority, string SourceText, string BadgeText, string TeamText, string ContactEmail, string EndsAt, string Version, string Notes, string Revision, string StatusLabel)
{
    public static ActionPlanMilestoneView ToActionPlanMilestoneView(ActionPlanMilestone item)
    {
        return new ActionPlanMilestoneView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Badge,
            Text.Truncate(item.Team, 120),
            item.ContactEmail,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanMilestoneView> ToActionPlanMilestoneViews(List<ActionPlanMilestone> items)
    {
        var views = new List<ActionPlanMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanMilestoneView(item));
        }
        return views;
    }

    public static double SumActionPlanMilestoneView(ActionPlanMilestoneView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedEndsAt + (double)parsedVersion;
    }
}
