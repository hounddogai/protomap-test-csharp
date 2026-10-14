namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending succession plan target.</summary>
public sealed record PendingSuccessionPlanTargetView(string CodeText, string Label, string ReviewerText, string Notes, string BatchSize, string Budget, string StatusLabel)
{
    public static PendingSuccessionPlanTargetView ToPendingSuccessionPlanTargetView(PendingSuccessionPlanTarget item)
    {
        return new PendingSuccessionPlanTargetView(
            item.Code,
            item.Label,
            item.Reviewer,
            Text.Truncate(item.Notes, 80),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingSuccessionPlanTargetView> ToPendingSuccessionPlanTargetViews(List<PendingSuccessionPlanTarget> items)
    {
        var views = new List<PendingSuccessionPlanTargetView>();
        foreach (var item in items)
        {
            views.Add(ToPendingSuccessionPlanTargetView(item));
        }
        return views;
    }

    public static double SumPendingSuccessionPlanTargetView(PendingSuccessionPlanTargetView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedBudget;
    }
}
