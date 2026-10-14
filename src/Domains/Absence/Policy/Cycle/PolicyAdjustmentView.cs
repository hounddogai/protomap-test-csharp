namespace Acme.Workforce.Absence.Policy.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy adjustment.</summary>
public sealed record PolicyAdjustmentView(string Code, string Title, string RateText, string BatchSize, string Unit, string Amount, string BudgetText, string Segment, string Capacity, string StatusLabel)
{
    public static PolicyAdjustmentView ToPolicyAdjustmentView(PolicyAdjustment item)
    {
        return new PolicyAdjustmentView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 64),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 120),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAdjustmentView> ToPolicyAdjustmentViews(List<PolicyAdjustment> items)
    {
        var views = new List<PolicyAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAdjustmentView(item));
        }
        return views;
    }

    public static double SumPolicyAdjustmentView(PolicyAdjustmentView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedBatchSize + parsedAmount;
    }
}
