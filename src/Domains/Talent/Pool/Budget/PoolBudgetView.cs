namespace Acme.Workforce.Talent.Pool.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool budget.</summary>
public sealed record PoolBudgetView(string Code, string Name, string BatchSize, string Ratio, string CostCenter, string OwnerId, string Locale, string DiscountText, string Amount, string AttemptsText, string StatusLabel)
{
    public static PoolBudgetView ToPoolBudgetView(PoolBudget item)
    {
        return new PoolBudgetView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.OwnerId,
            Text.Truncate(item.Locale, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolBudgetView> ToPoolBudgetViews(List<PoolBudget> items)
    {
        var views = new List<PoolBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToPoolBudgetView(item));
        }
        return views;
    }

    public static double SumPoolBudgetView(PoolBudgetView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedRatio + parsedDiscount;
    }
}
