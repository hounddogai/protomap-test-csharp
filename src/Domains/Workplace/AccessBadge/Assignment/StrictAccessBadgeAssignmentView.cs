namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict access badge assignment.</summary>
public sealed record StrictAccessBadgeAssignmentView(string Code, string Name, string Attempts, string Team, string UpdatedAt, string Skill, string Budget, string StatusLabel)
{
    public static StrictAccessBadgeAssignmentView ToStrictAccessBadgeAssignmentView(StrictAccessBadgeAssignment item)
    {
        return new StrictAccessBadgeAssignmentView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 120),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 64),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAccessBadgeAssignmentView> ToStrictAccessBadgeAssignmentViews(List<StrictAccessBadgeAssignment> items)
    {
        var views = new List<StrictAccessBadgeAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAccessBadgeAssignmentView(item));
        }
        return views;
    }

    public static double SumStrictAccessBadgeAssignmentView(StrictAccessBadgeAssignmentView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedUpdatedAt + parsedBudget;
    }
}
