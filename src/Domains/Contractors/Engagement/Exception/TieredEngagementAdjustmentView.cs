namespace Acme.Workforce.Contractors.Engagement.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered engagement adjustment.</summary>
public sealed record TieredEngagementAdjustmentView(string Code, string Name, string LabelText, string QuantityText, string EndsAt, string AmountText, string StatusLabel)
{
    public static TieredEngagementAdjustmentView ToTieredEngagementAdjustmentView(TieredEngagementAdjustment item)
    {
        return new TieredEngagementAdjustmentView(
            Text.Truncate(item.Code, 40),
            item.Name,
            Text.Truncate(item.Label, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredEngagementAdjustmentView> ToTieredEngagementAdjustmentViews(List<TieredEngagementAdjustment> items)
    {
        var views = new List<TieredEngagementAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToTieredEngagementAdjustmentView(item));
        }
        return views;
    }

    public static double SumTieredEngagementAdjustmentView(TieredEngagementAdjustmentView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedEndsAt + parsedAmount;
    }
}
