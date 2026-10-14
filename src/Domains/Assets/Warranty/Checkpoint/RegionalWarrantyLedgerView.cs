namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional warranty ledger.</summary>
public sealed record RegionalWarrantyLedgerView(string Code, string Title, string CostCenter, string WindowDaysText, string Amount, string Rate, string Name, string GraceDaysText, string Country, string StatusLabel)
{
    public static RegionalWarrantyLedgerView ToRegionalWarrantyLedgerView(RegionalWarrantyLedger item)
    {
        return new RegionalWarrantyLedgerView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.CostCenter,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalWarrantyLedgerView> ToRegionalWarrantyLedgerViews(List<RegionalWarrantyLedger> items)
    {
        var views = new List<RegionalWarrantyLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalWarrantyLedgerView(item));
        }
        return views;
    }

    public static double SumRegionalWarrantyLedgerView(RegionalWarrantyLedgerView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedAmount + parsedRate;
    }
}
