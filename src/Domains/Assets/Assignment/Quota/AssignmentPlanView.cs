namespace Acme.Workforce.Assets.Assignment.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment plan.</summary>
public sealed record AssignmentPlanView(string Code, string NameText, string SummaryText, string Region, string Badge, string TeamText, string Active, string Score, string StatusLabel)
{
    public static AssignmentPlanView ToAssignmentPlanView(AssignmentPlan item)
    {
        return new AssignmentPlanView(
            item.Code,
            item.Name,
            item.Summary,
            item.Region,
            item.Badge,
            Text.Truncate(item.Team, 120),
            $"{item.Active}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentPlanView> ToAssignmentPlanViews(List<AssignmentPlan> items)
    {
        var views = new List<AssignmentPlanView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentPlanView(item));
        }
        return views;
    }

    public static double SumAssignmentPlanView(AssignmentPlanView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
