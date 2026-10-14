namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedSpanLedgerChecks
{
    public static void BatchedSpanLedgerStartsInOpen()
    {
        var item = BatchedSpanLedger.NewBatchedSpanLedger("code-286", "title-64");
        Check.Equal(item.Status, BatchedSpanLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedSpanLedgerMovesToReopened()
    {
        var item = BatchedSpanLedger.NewBatchedSpanLedger("code-286", "title-64");
        try
        {
            item.MoveTo(BatchedSpanLedgerStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedSpanLedgerStatus.Reopened, "status");
    }

    public static void BatchedSpanLedgerRejectsMovesFromArchived()
    {
        var item = BatchedSpanLedger.NewBatchedSpanLedger("code-286", "title-64");
        item.Status = BatchedSpanLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedSpanLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedSpanLedgerStartsInOpen();
        BatchedSpanLedgerMovesToReopened();
        BatchedSpanLedgerRejectsMovesFromArchived();
    }
}
