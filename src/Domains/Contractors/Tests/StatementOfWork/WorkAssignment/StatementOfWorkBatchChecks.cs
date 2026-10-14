namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkAssignment;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>Checks of work assignment code, run by the project's runner.</summary>
public static class StatementOfWorkBatchChecks
{
    public static void StatementOfWorkBatchStartsInIdle()
    {
        var item = StatementOfWorkBatch.NewStatementOfWorkBatch("code-514", "label-820");
        Check.Equal(item.Status, StatementOfWorkBatchStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkBatchMovesToDraining()
    {
        var item = StatementOfWorkBatch.NewStatementOfWorkBatch("code-514", "label-820");
        try
        {
            item.MoveTo(StatementOfWorkBatchStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkBatchStatus.Draining, "status");
    }

    public static void StatementOfWorkBatchRejectsMovesFromStopped()
    {
        var item = StatementOfWorkBatch.NewStatementOfWorkBatch("code-514", "label-820");
        item.Status = StatementOfWorkBatchStatus.Stopped;
        Check.Fails(() => item.MoveTo(StatementOfWorkBatchStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkBatchStartsInIdle();
        StatementOfWorkBatchMovesToDraining();
        StatementOfWorkBatchRejectsMovesFromStopped();
    }
}
