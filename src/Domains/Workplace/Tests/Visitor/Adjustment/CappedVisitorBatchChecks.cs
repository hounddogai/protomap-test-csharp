namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedVisitorBatchChecks
{
    public static void CappedVisitorBatchStartsInRequested()
    {
        var item = CappedVisitorBatch.NewCappedVisitorBatch("code-677", "title-142");
        Check.Equal(item.Status, CappedVisitorBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedVisitorBatchMovesToGranted()
    {
        var item = CappedVisitorBatch.NewCappedVisitorBatch("code-677", "title-142");
        try
        {
            item.MoveTo(CappedVisitorBatchStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedVisitorBatchStatus.Granted, "status");
    }

    public static void CappedVisitorBatchRejectsMovesFromExpired()
    {
        var item = CappedVisitorBatch.NewCappedVisitorBatch("code-677", "title-142");
        item.Status = CappedVisitorBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedVisitorBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedVisitorBatchStartsInRequested();
        CappedVisitorBatchMovesToGranted();
        CappedVisitorBatchRejectsMovesFromExpired();
    }
}
