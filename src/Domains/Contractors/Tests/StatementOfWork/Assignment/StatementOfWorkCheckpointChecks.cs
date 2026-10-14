namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkCheckpointChecks
{
    public static void StatementOfWorkCheckpointStartsInIdle()
    {
        var item = StatementOfWorkCheckpoint.NewStatementOfWorkCheckpoint("code-9", "title-42");
        Check.Equal(item.Status, StatementOfWorkCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkCheckpointMovesToWarming()
    {
        var item = StatementOfWorkCheckpoint.NewStatementOfWorkCheckpoint("code-9", "title-42");
        try
        {
            item.MoveTo(StatementOfWorkCheckpointStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkCheckpointStatus.Warming, "status");
    }

    public static void StatementOfWorkCheckpointRejectsMovesFromStopped()
    {
        var item = StatementOfWorkCheckpoint.NewStatementOfWorkCheckpoint("code-9", "title-42");
        item.Status = StatementOfWorkCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(StatementOfWorkCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkCheckpointStartsInIdle();
        StatementOfWorkCheckpointMovesToWarming();
        StatementOfWorkCheckpointRejectsMovesFromStopped();
    }
}
