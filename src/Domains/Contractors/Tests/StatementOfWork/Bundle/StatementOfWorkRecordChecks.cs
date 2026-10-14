namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StatementOfWorkRecordChecks
{
    public static void StatementOfWorkRecordStartsInProposed()
    {
        var item = StatementOfWorkRecord.NewStatementOfWorkRecord("code-4", "name-63");
        Check.Equal(item.Status, StatementOfWorkRecordStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkRecordMovesToRejected()
    {
        var item = StatementOfWorkRecord.NewStatementOfWorkRecord("code-4", "name-63");
        try
        {
            item.MoveTo(StatementOfWorkRecordStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkRecordStatus.Rejected, "status");
    }

    public static void StatementOfWorkRecordRejectsMovesFromWithdrawn()
    {
        var item = StatementOfWorkRecord.NewStatementOfWorkRecord("code-4", "name-63");
        item.Status = StatementOfWorkRecordStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StatementOfWorkRecordStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkRecordStartsInProposed();
        StatementOfWorkRecordMovesToRejected();
        StatementOfWorkRecordRejectsMovesFromWithdrawn();
    }
}
