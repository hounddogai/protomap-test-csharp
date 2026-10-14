namespace Acme.Workforce.Assets.Laptop.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional laptop ledger.</summary>
public sealed record RegionalLaptopLedgerView(string Code, string Title, string ExpiresAtText, string AmountText, string QuantityText, string Threshold, string Cost, string StatusLabel)
{
    public static RegionalLaptopLedgerView ToRegionalLaptopLedgerView(RegionalLaptopLedger item)
    {
        return new RegionalLaptopLedgerView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalLaptopLedgerView> ToRegionalLaptopLedgerViews(List<RegionalLaptopLedger> items)
    {
        var views = new List<RegionalLaptopLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalLaptopLedgerView(item));
        }
        return views;
    }

    public static double SumRegionalLaptopLedgerView(RegionalLaptopLedgerView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedAmount + (double)parsedQuantity;
    }
}
