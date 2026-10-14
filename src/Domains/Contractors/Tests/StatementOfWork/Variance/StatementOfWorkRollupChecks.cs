namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Variance;

using Acme.Workforce.Contractors.StatementOfWork.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StatementOfWorkRollupChecks
{
    public static void StatementOfWorkRollupStartsInIdle()
    {
        var item = StatementOfWorkRollup.NewStatementOfWorkRollup("code-380", "label-820");
        Check.Equal(item.Status, StatementOfWorkRollupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkRollupMovesToDraining()
    {
        var item = StatementOfWorkRollup.NewStatementOfWorkRollup("code-380", "label-820");
        try
        {
            item.MoveTo(StatementOfWorkRollupStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkRollupStatus.Draining, "status");
    }

    public static void StatementOfWorkRollupRejectsMovesFromStopped()
    {
        var item = StatementOfWorkRollup.NewStatementOfWorkRollup("code-380", "label-820");
        item.Status = StatementOfWorkRollupStatus.Stopped;
        Check.Fails(() => item.MoveTo(StatementOfWorkRollupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkRollupStartsInIdle();
        StatementOfWorkRollupMovesToDraining();
        StatementOfWorkRollupRejectsMovesFromStopped();
    }
}
