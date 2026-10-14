namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class DefaultCareerPathExceptionChecks
{
    public static void DefaultCareerPathExceptionStartsInDraft()
    {
        var item = DefaultCareerPathException.NewDefaultCareerPathException("code-352", "label-131");
        Check.Equal(item.Status, DefaultCareerPathExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultCareerPathExceptionMovesToActive()
    {
        var item = DefaultCareerPathException.NewDefaultCareerPathException("code-352", "label-131");
        try
        {
            item.MoveTo(DefaultCareerPathExceptionStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultCareerPathExceptionStatus.Active, "status");
    }

    public static void DefaultCareerPathExceptionRejectsMovesFromClosed()
    {
        var item = DefaultCareerPathException.NewDefaultCareerPathException("code-352", "label-131");
        item.Status = DefaultCareerPathExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultCareerPathExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultCareerPathExceptionStartsInDraft();
        DefaultCareerPathExceptionMovesToActive();
        DefaultCareerPathExceptionRejectsMovesFromClosed();
    }
}
