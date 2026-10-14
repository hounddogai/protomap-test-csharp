namespace Acme.Workforce.Assets.Laptop.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending laptop ledger.</summary>
public sealed record PendingLaptopLedgerView(string Code, string Name, string RevisionText, string Pinned, string WindowDays, string Retries, string Cost, string Approved, string VisibleText, string CurrencyText, string StatusLabel)
{
    public static PendingLaptopLedgerView ToPendingLaptopLedgerView(PendingLaptopLedger item)
    {
        return new PendingLaptopLedgerView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Visible}",
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingLaptopLedgerView> ToPendingLaptopLedgerViews(List<PendingLaptopLedger> items)
    {
        var views = new List<PendingLaptopLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToPendingLaptopLedgerView(item));
        }
        return views;
    }

    public static double SumPendingLaptopLedgerView(PendingLaptopLedgerView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedWindowDays + (double)parsedRetries;
    }
}
