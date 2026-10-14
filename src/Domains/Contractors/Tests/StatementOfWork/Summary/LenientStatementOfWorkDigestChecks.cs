namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LenientStatementOfWorkDigestChecks
{
    public static void LenientStatementOfWorkDigestStartsInIdle()
    {
        var item = LenientStatementOfWorkDigest.NewLenientStatementOfWorkDigest("code-323", "title-975");
        Check.Equal(item.Status, LenientStatementOfWorkDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientStatementOfWorkDigestMovesToReady()
    {
        var item = LenientStatementOfWorkDigest.NewLenientStatementOfWorkDigest("code-323", "title-975");
        try
        {
            item.MoveTo(LenientStatementOfWorkDigestStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientStatementOfWorkDigestStatus.Ready, "status");
    }

    public static void LenientStatementOfWorkDigestRejectsMovesFromStopped()
    {
        var item = LenientStatementOfWorkDigest.NewLenientStatementOfWorkDigest("code-323", "title-975");
        item.Status = LenientStatementOfWorkDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientStatementOfWorkDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkDigestStartsInIdle();
        LenientStatementOfWorkDigestMovesToReady();
        LenientStatementOfWorkDigestRejectsMovesFromStopped();
    }
}
