namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedSpanLedgerViewChecks
{
    public static void BatchedSpanLedgerViewShowsTheStatusInCapitals()
    {
        var item = BatchedSpanLedger.NewBatchedSpanLedger("code-286", "title-64");
        var view = BatchedSpanLedgerView.ToBatchedSpanLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedSpanLedgerView.ToBatchedSpanLedgerViews(new List<BatchedSpanLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedSpanLedgerViewShowsTheStatusInCapitals();
    }
}
