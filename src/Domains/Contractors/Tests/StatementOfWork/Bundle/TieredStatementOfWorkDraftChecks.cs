namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredStatementOfWorkDraftChecks
{
    public static void TieredStatementOfWorkDraftStartsInPlanned()
    {
        var item = TieredStatementOfWorkDraft.NewTieredStatementOfWorkDraft("code-164", "name-678");
        Check.Equal(item.Status, TieredStatementOfWorkDraftStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredStatementOfWorkDraftMovesToCompleted()
    {
        var item = TieredStatementOfWorkDraft.NewTieredStatementOfWorkDraft("code-164", "name-678");
        try
        {
            item.MoveTo(TieredStatementOfWorkDraftStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredStatementOfWorkDraftStatus.Completed, "status");
    }

    public static void TieredStatementOfWorkDraftRejectsMovesFromCancelled()
    {
        var item = TieredStatementOfWorkDraft.NewTieredStatementOfWorkDraft("code-164", "name-678");
        item.Status = TieredStatementOfWorkDraftStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredStatementOfWorkDraftStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredStatementOfWorkDraftStartsInPlanned();
        TieredStatementOfWorkDraftMovesToCompleted();
        TieredStatementOfWorkDraftRejectsMovesFromCancelled();
    }
}
