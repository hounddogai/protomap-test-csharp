namespace Acme.Workforce.Payrollops.Tests.PayRun.RunRecord;

using Acme.Workforce.Payrollops.PayRun.RunRecord;

/// <summary>Checks of run record code, run by the project's runner.</summary>
public static class CappedPayRunRecordChecks
{
    public static void CappedPayRunRecordStartsInDraft()
    {
        var item = CappedPayRunRecord.NewCappedPayRunRecord("code-986", "name-43");
        Check.Equal(item.Status, CappedPayRunRecordStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPayRunRecordMovesToPending()
    {
        var item = CappedPayRunRecord.NewCappedPayRunRecord("code-986", "name-43");
        try
        {
            item.MoveTo(CappedPayRunRecordStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPayRunRecordStatus.Pending, "status");
    }

    public static void CappedPayRunRecordRejectsMovesFromClosed()
    {
        var item = CappedPayRunRecord.NewCappedPayRunRecord("code-986", "name-43");
        item.Status = CappedPayRunRecordStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedPayRunRecordStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedPayRunRecordStartsInDraft();
        CappedPayRunRecordMovesToPending();
        CappedPayRunRecordRejectsMovesFromClosed();
    }
}
