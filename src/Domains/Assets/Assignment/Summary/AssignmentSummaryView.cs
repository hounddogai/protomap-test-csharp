namespace Acme.Workforce.Assets.Assignment.Summary;

using System.Globalization;

/// <summary>How screens and exports show a assignment summary.</summary>
public sealed record AssignmentSummaryView(string Code, string Name, string Badge, string TenureMonthsText, string Source, string Budget, string Retries, string UpdatedAtText, string StatusLabel)
{
    public static AssignmentSummaryView ToAssignmentSummaryView(AssignmentSummary item)
    {
        return new AssignmentSummaryView(
            item.Code,
            item.Name,
            item.Badge,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentSummaryView> ToAssignmentSummaryViews(List<AssignmentSummary> items)
    {
        var views = new List<AssignmentSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentSummaryView(item));
        }
        return views;
    }

    public static double SumAssignmentSummaryView(AssignmentSummaryView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedBudget + (double)parsedRetries;
    }
}
