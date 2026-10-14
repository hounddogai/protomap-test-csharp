namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class DefaultExceptionDigestChecks
{
    public static void DefaultExceptionDigestStartsInOpen()
    {
        var item = DefaultExceptionDigest.NewDefaultExceptionDigest("code-533", "label-981");
        Check.Equal(item.Status, DefaultExceptionDigestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultExceptionDigestMovesToInReview()
    {
        var item = DefaultExceptionDigest.NewDefaultExceptionDigest("code-533", "label-981");
        try
        {
            item.MoveTo(DefaultExceptionDigestStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultExceptionDigestStatus.InReview, "status");
    }

    public static void DefaultExceptionDigestRejectsMovesFromArchived()
    {
        var item = DefaultExceptionDigest.NewDefaultExceptionDigest("code-533", "label-981");
        item.Status = DefaultExceptionDigestStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultExceptionDigestStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultExceptionDigestStartsInOpen();
        DefaultExceptionDigestMovesToInReview();
        DefaultExceptionDigestRejectsMovesFromArchived();
    }
}
