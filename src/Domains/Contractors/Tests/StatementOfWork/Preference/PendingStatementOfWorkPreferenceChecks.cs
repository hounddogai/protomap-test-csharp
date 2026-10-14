namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingStatementOfWorkPreferenceChecks
{
    public static void PendingStatementOfWorkPreferenceStartsInNew()
    {
        var item = PendingStatementOfWorkPreference.NewPendingStatementOfWorkPreference("code-457", "name-678");
        Check.Equal(item.Status, PendingStatementOfWorkPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingStatementOfWorkPreferenceMovesToFailed()
    {
        var item = PendingStatementOfWorkPreference.NewPendingStatementOfWorkPreference("code-457", "name-678");
        try
        {
            item.MoveTo(PendingStatementOfWorkPreferenceStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingStatementOfWorkPreferenceStatus.Failed, "status");
    }

    public static void PendingStatementOfWorkPreferenceRejectsMovesFromFailed()
    {
        var item = PendingStatementOfWorkPreference.NewPendingStatementOfWorkPreference("code-457", "name-678");
        item.Status = PendingStatementOfWorkPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingStatementOfWorkPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingStatementOfWorkPreferenceStartsInNew();
        PendingStatementOfWorkPreferenceMovesToFailed();
        PendingStatementOfWorkPreferenceRejectsMovesFromFailed();
    }
}
