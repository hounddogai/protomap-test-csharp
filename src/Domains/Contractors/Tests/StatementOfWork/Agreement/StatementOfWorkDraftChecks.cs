namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class StatementOfWorkDraftChecks
{
    public static void StatementOfWorkDraftStartsInNew()
    {
        var item = StatementOfWorkDraft.NewStatementOfWorkDraft("code-536", "label-90");
        Check.Equal(item.Status, StatementOfWorkDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkDraftMovesToRunning()
    {
        var item = StatementOfWorkDraft.NewStatementOfWorkDraft("code-536", "label-90");
        try
        {
            item.MoveTo(StatementOfWorkDraftStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkDraftStatus.Running, "status");
    }

    public static void StatementOfWorkDraftRejectsMovesFromFailed()
    {
        var item = StatementOfWorkDraft.NewStatementOfWorkDraft("code-536", "label-90");
        item.Status = StatementOfWorkDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(StatementOfWorkDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkDraftStartsInNew();
        StatementOfWorkDraftMovesToRunning();
        StatementOfWorkDraftRejectsMovesFromFailed();
    }
}
