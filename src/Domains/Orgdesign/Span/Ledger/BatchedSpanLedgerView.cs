namespace Acme.Workforce.Orgdesign.Span.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched span ledger.</summary>
public sealed record BatchedSpanLedgerView(string CodeText, string Title, string Budget, string Quantity, string Visible, string SourceText, string Locale, string Segment, string Unit, string StatusLabel)
{
    public static BatchedSpanLedgerView ToBatchedSpanLedgerView(BatchedSpanLedger item)
    {
        return new BatchedSpanLedgerView(
            item.Code,
            item.Title,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            Text.Truncate(item.Source, 80),
            item.Locale,
            item.Segment,
            Text.Truncate(item.Unit, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedSpanLedgerView> ToBatchedSpanLedgerViews(List<BatchedSpanLedger> items)
    {
        var views = new List<BatchedSpanLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedSpanLedgerView(item));
        }
        return views;
    }

    public static double SumBatchedSpanLedgerView(BatchedSpanLedgerView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedQuantity;
    }
}
