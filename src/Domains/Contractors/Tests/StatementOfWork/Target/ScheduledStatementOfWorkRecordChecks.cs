namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkRecordChecks
{
    public static void ScheduledStatementOfWorkRecordStartsInOpen()
    {
        var item = ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord("code-240", "name-371");
        Check.Equal(item.Status, ScheduledStatementOfWorkRecordStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledStatementOfWorkRecordMovesToReopened()
    {
        var item = ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord("code-240", "name-371");
        try
        {
            item.MoveTo(ScheduledStatementOfWorkRecordStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledStatementOfWorkRecordStatus.Reopened, "status");
    }

    public static void ScheduledStatementOfWorkRecordRejectsMovesFromArchived()
    {
        var item = ScheduledStatementOfWorkRecord.NewScheduledStatementOfWorkRecord("code-240", "name-371");
        item.Status = ScheduledStatementOfWorkRecordStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledStatementOfWorkRecordStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkRecordStartsInOpen();
        ScheduledStatementOfWorkRecordMovesToReopened();
        ScheduledStatementOfWorkRecordRejectsMovesFromArchived();
    }
}
