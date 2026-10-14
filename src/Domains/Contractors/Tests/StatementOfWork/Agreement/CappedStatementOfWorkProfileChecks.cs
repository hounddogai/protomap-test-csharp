namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProfileChecks
{
    public static void CappedStatementOfWorkProfileStartsInNew()
    {
        var item = CappedStatementOfWorkProfile.NewCappedStatementOfWorkProfile("code-988", "title-387");
        Check.Equal(item.Status, CappedStatementOfWorkProfileStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedStatementOfWorkProfileMovesToRunning()
    {
        var item = CappedStatementOfWorkProfile.NewCappedStatementOfWorkProfile("code-988", "title-387");
        try
        {
            item.MoveTo(CappedStatementOfWorkProfileStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedStatementOfWorkProfileStatus.Running, "status");
    }

    public static void CappedStatementOfWorkProfileRejectsMovesFromFailed()
    {
        var item = CappedStatementOfWorkProfile.NewCappedStatementOfWorkProfile("code-988", "title-387");
        item.Status = CappedStatementOfWorkProfileStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedStatementOfWorkProfileStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkProfileStartsInNew();
        CappedStatementOfWorkProfileMovesToRunning();
        CappedStatementOfWorkProfileRejectsMovesFromFailed();
    }
}
