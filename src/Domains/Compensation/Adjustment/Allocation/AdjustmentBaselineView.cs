namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using System.Globalization;

/// <summary>How screens and exports show a adjustment baseline.</summary>
public sealed record AdjustmentBaselineView(string Code, string Title, string GraceDaysText, string Threshold, string Budget, string StatusLabel)
{
    public static AdjustmentBaselineView ToAdjustmentBaselineView(AdjustmentBaseline item)
    {
        return new AdjustmentBaselineView(
            item.Code,
            item.Title,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentBaselineView> ToAdjustmentBaselineViews(List<AdjustmentBaseline> items)
    {
        var views = new List<AdjustmentBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentBaselineView(item));
        }
        return views;
    }

    public static double SumAdjustmentBaselineView(AdjustmentBaselineView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedThreshold + parsedBudget;
    }
}
