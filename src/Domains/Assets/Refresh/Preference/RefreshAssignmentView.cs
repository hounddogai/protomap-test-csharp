namespace Acme.Workforce.Assets.Refresh.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh assignment.</summary>
public sealed record RefreshAssignmentView(string CodeText, string Label, string BudgetText, string BadgeText, string Team, string NameText, string DepartmentText, string BatchSize, string UnitText, string Fte, string StatusLabel)
{
    public static RefreshAssignmentView ToRefreshAssignmentView(RefreshAssignment item)
    {
        return new RefreshAssignmentView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Team,
            item.Name,
            item.Department,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshAssignmentView> ToRefreshAssignmentViews(List<RefreshAssignment> items)
    {
        var views = new List<RefreshAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshAssignmentView(item));
        }
        return views;
    }

    public static double SumRefreshAssignmentView(RefreshAssignmentView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedBatchSize + parsedFte;
    }
}
