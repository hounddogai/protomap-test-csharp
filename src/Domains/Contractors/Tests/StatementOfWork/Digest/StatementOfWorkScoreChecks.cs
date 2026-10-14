namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkScoreChecks
{
    public static void StatementOfWorkScoreStartsInRequested()
    {
        var item = StatementOfWorkScore.NewStatementOfWorkScore("code-628", "name-475");
        Check.Equal(item.Status, StatementOfWorkScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkScoreMovesToGranted()
    {
        var item = StatementOfWorkScore.NewStatementOfWorkScore("code-628", "name-475");
        try
        {
            item.MoveTo(StatementOfWorkScoreStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkScoreStatus.Granted, "status");
    }

    public static void StatementOfWorkScoreRejectsMovesFromExpired()
    {
        var item = StatementOfWorkScore.NewStatementOfWorkScore("code-628", "name-475");
        item.Status = StatementOfWorkScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(StatementOfWorkScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkScoreStartsInRequested();
        StatementOfWorkScoreMovesToGranted();
        StatementOfWorkScoreRejectsMovesFromExpired();
    }
}
