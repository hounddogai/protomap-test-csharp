namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan metric.</summary>
public sealed record ActionPlanMetricView(string CodeText, string Title, string Revision, string Skill, string JobCode, string StatusLabel)
{
    public static ActionPlanMetricView ToActionPlanMetricView(ActionPlanMetric item)
    {
        return new ActionPlanMetricView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.JobCode, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanMetricView> ToActionPlanMetricViews(List<ActionPlanMetric> items)
    {
        var views = new List<ActionPlanMetricView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanMetricView(item));
        }
        return views;
    }

    public static double SumActionPlanMetricView(ActionPlanMetricView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
