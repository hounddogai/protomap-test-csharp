namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global adjustment bundle.</summary>
public sealed record GlobalAdjustmentBundleView(string Code, string Name, string Rate, string Rating, string BudgetText, string StatusLabel)
{
    public static GlobalAdjustmentBundleView ToGlobalAdjustmentBundleView(GlobalAdjustmentBundle item)
    {
        return new GlobalAdjustmentBundleView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalAdjustmentBundleView> ToGlobalAdjustmentBundleViews(List<GlobalAdjustmentBundle> items)
    {
        var views = new List<GlobalAdjustmentBundleView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalAdjustmentBundleView(item));
        }
        return views;
    }

    public static double SumGlobalAdjustmentBundleView(GlobalAdjustmentBundleView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedRate + parsedRating + parsedBudget;
    }
}
