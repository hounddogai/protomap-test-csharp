namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkManifestChecks
{
    public static void StatementOfWorkManifestStartsInIdle()
    {
        var item = StatementOfWorkManifest.NewStatementOfWorkManifest("code-714", "title-907");
        Check.Equal(item.Status, StatementOfWorkManifestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkManifestMovesToReady()
    {
        var item = StatementOfWorkManifest.NewStatementOfWorkManifest("code-714", "title-907");
        try
        {
            item.MoveTo(StatementOfWorkManifestStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkManifestStatus.Ready, "status");
    }

    public static void StatementOfWorkManifestRejectsMovesFromStopped()
    {
        var item = StatementOfWorkManifest.NewStatementOfWorkManifest("code-714", "title-907");
        item.Status = StatementOfWorkManifestStatus.Stopped;
        Check.Fails(() => item.MoveTo(StatementOfWorkManifestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkManifestStartsInIdle();
        StatementOfWorkManifestMovesToReady();
        StatementOfWorkManifestRejectsMovesFromStopped();
    }
}
