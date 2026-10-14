namespace Acme.Workforce.Compensation.Adjustment.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched adjustment rule.</summary>
public sealed record BatchedAdjustmentRuleView(string Code, string TitleText, string Team, string SalaryBand, string Budget, string Maximum, string Category, string Cost, string Threshold, string StatusLabel)
{
    public static BatchedAdjustmentRuleView ToBatchedAdjustmentRuleView(BatchedAdjustmentRule item)
    {
        return new BatchedAdjustmentRuleView(
            item.Code,
            item.Title,
            Text.Truncate(item.Team, 64),
            item.SalaryBand,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 64),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedAdjustmentRuleView> ToBatchedAdjustmentRuleViews(List<BatchedAdjustmentRule> items)
    {
        var views = new List<BatchedAdjustmentRuleView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedAdjustmentRuleView(item));
        }
        return views;
    }

    public static double SumBatchedAdjustmentRuleView(BatchedAdjustmentRuleView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedBudget + parsedMaximum + parsedCost;
    }
}
