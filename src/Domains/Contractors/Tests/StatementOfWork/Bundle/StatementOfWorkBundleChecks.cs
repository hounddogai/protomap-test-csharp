namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StatementOfWorkBundleChecks
{
    public static void StatementOfWorkBundleStartsInDraft()
    {
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("code-551", "name-155");
        Check.Equal(item.Status, StatementOfWorkBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkBundleMovesToPending()
    {
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("code-551", "name-155");
        try
        {
            item.MoveTo(StatementOfWorkBundleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkBundleStatus.Pending, "status");
    }

    public static void StatementOfWorkBundleRejectsMovesFromClosed()
    {
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("code-551", "name-155");
        item.Status = StatementOfWorkBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(StatementOfWorkBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkBundleStartsInDraft();
        StatementOfWorkBundleMovesToPending();
        StatementOfWorkBundleRejectsMovesFromClosed();
    }
}
