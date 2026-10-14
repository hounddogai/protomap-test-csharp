namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolExceptionChecks
{
    public static void DefaultPoolExceptionStartsInCreated()
    {
        var item = DefaultPoolException.NewDefaultPoolException("code-237", "title-233");
        Check.Equal(item.Status, DefaultPoolExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPoolExceptionMovesToPublished()
    {
        var item = DefaultPoolException.NewDefaultPoolException("code-237", "title-233");
        try
        {
            item.MoveTo(DefaultPoolExceptionStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPoolExceptionStatus.Published, "status");
    }

    public static void DefaultPoolExceptionRejectsMovesFromRetired()
    {
        var item = DefaultPoolException.NewDefaultPoolException("code-237", "title-233");
        item.Status = DefaultPoolExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultPoolExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultPoolExceptionStartsInCreated();
        DefaultPoolExceptionMovesToPublished();
        DefaultPoolExceptionRejectsMovesFromRetired();
    }
}
