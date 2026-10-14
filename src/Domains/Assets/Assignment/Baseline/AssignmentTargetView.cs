namespace Acme.Workforce.Assets.Assignment.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment target.</summary>
public sealed record AssignmentTargetView(string CodeText, string Label, string OwnerIdText, string Minimum, string ArchivedText, string JobCode, string Name, string Budget, string Skill, string EmployeeIdText, string StatusLabel)
{
    public static AssignmentTargetView ToAssignmentTargetView(AssignmentTarget item)
    {
        return new AssignmentTargetView(
            Text.Truncate(item.Code, 80),
            item.Label,
            Text.Truncate(item.OwnerId, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.JobCode,
            item.Name,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentTargetView> ToAssignmentTargetViews(List<AssignmentTarget> items)
    {
        var views = new List<AssignmentTargetView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentTargetView(item));
        }
        return views;
    }

    public static double SumAssignmentTargetView(AssignmentTargetView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedBudget;
    }
}
