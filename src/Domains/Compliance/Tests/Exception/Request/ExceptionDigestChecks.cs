namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionDigestChecks
{
    public static void ExceptionDigestStartsInDraft()
    {
        var item = ExceptionDigest.NewExceptionDigest("code-412", "name-333");
        Check.Equal(item.Status, ExceptionDigestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionDigestMovesToActive()
    {
        var item = ExceptionDigest.NewExceptionDigest("code-412", "name-333");
        try
        {
            item.MoveTo(ExceptionDigestStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionDigestStatus.Active, "status");
    }

    public static void ExceptionDigestRejectsMovesFromClosed()
    {
        var item = ExceptionDigest.NewExceptionDigest("code-412", "name-333");
        item.Status = ExceptionDigestStatus.Closed;
        Check.Fails(() => item.MoveTo(ExceptionDigestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ExceptionDigestStartsInDraft();
        ExceptionDigestMovesToActive();
        ExceptionDigestRejectsMovesFromClosed();
    }
}
