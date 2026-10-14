namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class DefaultRateDraftChecks
{
    public static void DefaultRateDraftStartsInOpen()
    {
        var item = DefaultRateDraft.NewDefaultRateDraft("code-837", "name-648");
        Check.Equal(item.Status, DefaultRateDraftStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultRateDraftMovesToResolved()
    {
        var item = DefaultRateDraft.NewDefaultRateDraft("code-837", "name-648");
        try
        {
            item.MoveTo(DefaultRateDraftStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultRateDraftStatus.Resolved, "status");
    }

    public static void DefaultRateDraftRejectsMovesFromArchived()
    {
        var item = DefaultRateDraft.NewDefaultRateDraft("code-837", "name-648");
        item.Status = DefaultRateDraftStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultRateDraftStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultRateDraftStartsInOpen();
        DefaultRateDraftMovesToResolved();
        DefaultRateDraftRejectsMovesFromArchived();
    }
}
