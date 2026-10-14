namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalStatementOfWorkTargetChecks
{
    public static void GlobalStatementOfWorkTargetStartsInNew()
    {
        var item = GlobalStatementOfWorkTarget.NewGlobalStatementOfWorkTarget("code-789", "name-873");
        Check.Equal(item.Status, GlobalStatementOfWorkTargetStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalStatementOfWorkTargetMovesToDone()
    {
        var item = GlobalStatementOfWorkTarget.NewGlobalStatementOfWorkTarget("code-789", "name-873");
        try
        {
            item.MoveTo(GlobalStatementOfWorkTargetStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalStatementOfWorkTargetStatus.Done, "status");
    }

    public static void GlobalStatementOfWorkTargetRejectsMovesFromFailed()
    {
        var item = GlobalStatementOfWorkTarget.NewGlobalStatementOfWorkTarget("code-789", "name-873");
        item.Status = GlobalStatementOfWorkTargetStatus.Failed;
        Check.Fails(() => item.MoveTo(GlobalStatementOfWorkTargetStatus.New), "move");
    }

    public static void RunAll()
    {
        GlobalStatementOfWorkTargetStartsInNew();
        GlobalStatementOfWorkTargetMovesToDone();
        GlobalStatementOfWorkTargetRejectsMovesFromFailed();
    }
}
