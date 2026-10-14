namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using System.Globalization;

/// <summary>How screens and exports show a adjustment item.</summary>
public sealed record AdjustmentItemView(string CodeText, string TitleText, string Name, string Region, string UpdatedAt, string Margin, string BudgetText, string StatusLabel)
{
    public static AdjustmentItemView ToAdjustmentItemView(AdjustmentItem item)
    {
        return new AdjustmentItemView(
            item.Code,
            item.Title,
            item.Name,
            item.Region,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentItemView> ToAdjustmentItemViews(List<AdjustmentItem> items)
    {
        var views = new List<AdjustmentItemView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentItemView(item));
        }
        return views;
    }

    public static double SumAdjustmentItemView(AdjustmentItemView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedMargin + parsedBudget;
    }
}
