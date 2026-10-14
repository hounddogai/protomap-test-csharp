namespace Acme.Workforce.Contractors.Tests.StatementOfWork.RecordItem;

using Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkRecordChecks
{
    public static void LegacyStatementOfWorkRecordStartsInIdle()
    {
        var item = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord("code-14", "name-98");
        Check.Equal(item.Status, LegacyStatementOfWorkRecordStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyStatementOfWorkRecordMovesToDraining()
    {
        var item = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord("code-14", "name-98");
        try
        {
            item.MoveTo(LegacyStatementOfWorkRecordStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyStatementOfWorkRecordStatus.Draining, "status");
    }

    public static void LegacyStatementOfWorkRecordRejectsMovesFromStopped()
    {
        var item = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord("code-14", "name-98");
        item.Status = LegacyStatementOfWorkRecordStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyStatementOfWorkRecordStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyStatementOfWorkRecordStartsInIdle();
        LegacyStatementOfWorkRecordMovesToDraining();
        LegacyStatementOfWorkRecordRejectsMovesFromStopped();
    }
}
