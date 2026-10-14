namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan baseline.</summary>
public sealed record ActionPlanBaselineView(string Code, string Label, string Notes, string VersionText, string SummaryText, string Category, string GraceDays, string Reviewer, string Location, string Phone, string Sequence, string StatusLabel)
{
    public static ActionPlanBaselineView ToActionPlanBaselineView(ActionPlanBaseline item)
    {
        return new ActionPlanBaselineView(
            item.Code,
            item.Label,
            item.Notes,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Category,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 80),
            Text.Truncate(item.Location, 120),
            item.Phone,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanBaselineView> ToActionPlanBaselineViews(List<ActionPlanBaseline> items)
    {
        var views = new List<ActionPlanBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanBaselineView(item));
        }
        return views;
    }

    public static double SumActionPlanBaselineView(ActionPlanBaselineView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedGraceDays + (double)parsedSequence;
    }
}
