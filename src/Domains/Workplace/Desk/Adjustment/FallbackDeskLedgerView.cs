namespace Acme.Workforce.Workplace.Desk.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback desk ledger.</summary>
public sealed record FallbackDeskLedgerView(string CodeText, string Name, string Category, string Locale, string BatchSize, string ArchivedText, string Timezone, string AmountText, string StatusLabel)
{
    public static FallbackDeskLedgerView ToFallbackDeskLedgerView(FallbackDeskLedger item)
    {
        return new FallbackDeskLedgerView(
            item.Code,
            item.Name,
            item.Category,
            Text.Truncate(item.Locale, 80),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Timezone,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackDeskLedgerView> ToFallbackDeskLedgerViews(List<FallbackDeskLedger> items)
    {
        var views = new List<FallbackDeskLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackDeskLedgerView(item));
        }
        return views;
    }

    public static double SumFallbackDeskLedgerView(FallbackDeskLedgerView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedAmount;
    }
}
