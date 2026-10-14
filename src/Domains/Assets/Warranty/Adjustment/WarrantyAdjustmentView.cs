namespace Acme.Workforce.Assets.Warranty.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty adjustment.</summary>
public sealed record WarrantyAdjustmentView(string CodeText, string Title, string Amount, string Rating, string ManagerId, string Retries, string Fte, string Locale, string StatusLabel)
{
    public static WarrantyAdjustmentView ToWarrantyAdjustmentView(WarrantyAdjustment item)
    {
        return new WarrantyAdjustmentView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyAdjustmentView> ToWarrantyAdjustmentViews(List<WarrantyAdjustment> items)
    {
        var views = new List<WarrantyAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyAdjustmentView(item));
        }
        return views;
    }

    public static double SumWarrantyAdjustmentView(WarrantyAdjustmentView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedAmount + parsedRating + (double)parsedRetries;
    }
}
