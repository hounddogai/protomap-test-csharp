namespace Acme.Workforce.Assets.Assignment.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient assignment score.</summary>
public sealed record LenientAssignmentScoreView(string Code, string Label, string CategoryText, string PriorityText, string CostCenter, string Department, string BudgetText, string Reviewer, string Cost, string StatusLabel)
{
    public static LenientAssignmentScoreView ToLenientAssignmentScoreView(LenientAssignmentScore item)
    {
        return new LenientAssignmentScoreView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Category, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Department,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientAssignmentScoreView> ToLenientAssignmentScoreViews(List<LenientAssignmentScore> items)
    {
        var views = new List<LenientAssignmentScoreView>();
        foreach (var item in items)
        {
            views.Add(ToLenientAssignmentScoreView(item));
        }
        return views;
    }

    public static double SumLenientAssignmentScoreView(LenientAssignmentScoreView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedBudget + parsedCost;
    }
}
