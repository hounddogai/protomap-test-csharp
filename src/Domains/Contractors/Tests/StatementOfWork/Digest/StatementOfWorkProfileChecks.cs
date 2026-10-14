namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkProfileChecks
{
    public static void StatementOfWorkProfileStartsInIdle()
    {
        var item = StatementOfWorkProfile.NewStatementOfWorkProfile("code-59", "label-832");
        Check.Equal(item.Status, StatementOfWorkProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkProfileMovesToReady()
    {
        var item = StatementOfWorkProfile.NewStatementOfWorkProfile("code-59", "label-832");
        try
        {
            item.MoveTo(StatementOfWorkProfileStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkProfileStatus.Ready, "status");
    }

    public static void StatementOfWorkProfileRejectsMovesFromStopped()
    {
        var item = StatementOfWorkProfile.NewStatementOfWorkProfile("code-59", "label-832");
        item.Status = StatementOfWorkProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(StatementOfWorkProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkProfileStartsInIdle();
        StatementOfWorkProfileMovesToReady();
        StatementOfWorkProfileRejectsMovesFromStopped();
    }
}
