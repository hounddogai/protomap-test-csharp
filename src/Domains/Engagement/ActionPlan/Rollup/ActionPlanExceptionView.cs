namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan exception.</summary>
public sealed record ActionPlanExceptionView(string Code, string Title, string Threshold, string Segment, string Unit, string Reviewer, string Skill, string Margin, string EmployeeId, string StatusLabel)
{
    public static ActionPlanExceptionView ToActionPlanExceptionView(ActionPlanException item)
    {
        return new ActionPlanExceptionView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 64),
            Text.Truncate(item.Unit, 120),
            Text.Truncate(item.Reviewer, 80),
            item.Skill,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanExceptionView> ToActionPlanExceptionViews(List<ActionPlanException> items)
    {
        var views = new List<ActionPlanExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanExceptionView(item));
        }
        return views;
    }

    public static double SumActionPlanExceptionView(ActionPlanExceptionView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedMargin;
    }
}
