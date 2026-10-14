namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedReturnPlanLedgerChecks
{
    public static void BatchedReturnPlanLedgerStartsInDraft()
    {
        var item = BatchedReturnPlanLedger.NewBatchedReturnPlanLedger("code-338", "name-811");
        Check.Equal(item.Status, BatchedReturnPlanLedgerStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedReturnPlanLedgerMovesToSuspended()
    {
        var item = BatchedReturnPlanLedger.NewBatchedReturnPlanLedger("code-338", "name-811");
        try
        {
            item.MoveTo(BatchedReturnPlanLedgerStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedReturnPlanLedgerStatus.Suspended, "status");
    }

    public static void BatchedReturnPlanLedgerRejectsMovesFromClosed()
    {
        var item = BatchedReturnPlanLedger.NewBatchedReturnPlanLedger("code-338", "name-811");
        item.Status = BatchedReturnPlanLedgerStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedReturnPlanLedgerStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedReturnPlanLedgerStartsInDraft();
        BatchedReturnPlanLedgerMovesToSuspended();
        BatchedReturnPlanLedgerRejectsMovesFromClosed();
    }
}
