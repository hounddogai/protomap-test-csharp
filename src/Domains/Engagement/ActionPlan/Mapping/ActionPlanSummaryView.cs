namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan summary.</summary>
public sealed record ActionPlanSummaryView(string Code, string Name, string SkillText, string Reviewer, string Attempts, string SequenceText, string StatusLabel)
{
    public static ActionPlanSummaryView ToActionPlanSummaryView(ActionPlanSummary item)
    {
        return new ActionPlanSummaryView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 64),
            item.Skill,
            item.Reviewer,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanSummaryView> ToActionPlanSummaryViews(List<ActionPlanSummary> items)
    {
        var views = new List<ActionPlanSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanSummaryView(item));
        }
        return views;
    }

    public static double SumActionPlanSummaryView(ActionPlanSummaryView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedSequence;
    }
}
