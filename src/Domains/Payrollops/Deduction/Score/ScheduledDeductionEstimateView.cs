namespace Acme.Workforce.Payrollops.Deduction.Score;

using System.Globalization;

/// <summary>How screens and exports show a scheduled deduction estimate.</summary>
public sealed record ScheduledDeductionEstimateView(string Code, string Name, string ManagerId, string Budget, string StatusLabel)
{
    public static ScheduledDeductionEstimateView ToScheduledDeductionEstimateView(ScheduledDeductionEstimate item)
    {
        return new ScheduledDeductionEstimateView(
            item.Code,
            item.Name,
            item.ManagerId,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledDeductionEstimateView> ToScheduledDeductionEstimateViews(List<ScheduledDeductionEstimate> items)
    {
        var views = new List<ScheduledDeductionEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledDeductionEstimateView(item));
        }
        return views;
    }

    public static double SumScheduledDeductionEstimateView(ScheduledDeductionEstimateView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
