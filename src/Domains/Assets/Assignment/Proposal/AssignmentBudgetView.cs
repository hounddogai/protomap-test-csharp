namespace Acme.Workforce.Assets.Assignment.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment budget.</summary>
public sealed record AssignmentBudgetView(string CodeText, string Label, string EmployeeId, string Rate, string Archived, string SourceText, string BatchSize, string Fte, string StatusLabel)
{
    public static AssignmentBudgetView ToAssignmentBudgetView(AssignmentBudget item)
    {
        return new AssignmentBudgetView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.EmployeeId,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Source,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentBudgetView> ToAssignmentBudgetViews(List<AssignmentBudget> items)
    {
        var views = new List<AssignmentBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentBudgetView(item));
        }
        return views;
    }

    public static double SumAssignmentBudgetView(AssignmentBudgetView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedBatchSize + parsedFte;
    }
}
