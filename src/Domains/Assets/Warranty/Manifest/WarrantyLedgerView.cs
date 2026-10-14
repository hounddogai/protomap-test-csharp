namespace Acme.Workforce.Assets.Warranty.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty ledger.</summary>
public sealed record WarrantyLedgerView(string Code, string Title, string Score, string SalaryBand, string Minimum, string Description, string StatusLabel)
{
    public static WarrantyLedgerView ToWarrantyLedgerView(WarrantyLedger item)
    {
        return new WarrantyLedgerView(
            item.Code,
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyLedgerView> ToWarrantyLedgerViews(List<WarrantyLedger> items)
    {
        var views = new List<WarrantyLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyLedgerView(item));
        }
        return views;
    }

    public static double SumWarrantyLedgerView(WarrantyLedgerView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedScore + parsedMinimum;
    }
}
