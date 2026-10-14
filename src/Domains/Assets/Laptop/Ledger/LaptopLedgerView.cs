namespace Acme.Workforce.Assets.Laptop.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop ledger.</summary>
public sealed record LaptopLedgerView(string CodeText, string Label, string CostCenter, string Revision, string WindowDays, string RatingText, string Currency, string StatusLabel)
{
    public static LaptopLedgerView ToLaptopLedgerView(LaptopLedger item)
    {
        return new LaptopLedgerView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 64),
            item.CostCenter,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopLedgerView> ToLaptopLedgerViews(List<LaptopLedger> items)
    {
        var views = new List<LaptopLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopLedgerView(item));
        }
        return views;
    }

    public static double SumLaptopLedgerView(LaptopLedgerView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedWindowDays + parsedRating;
    }
}
