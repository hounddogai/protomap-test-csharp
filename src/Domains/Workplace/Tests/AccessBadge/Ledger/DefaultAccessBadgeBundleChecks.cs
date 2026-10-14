namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class DefaultAccessBadgeBundleChecks
{
    public static void DefaultAccessBadgeBundleStartsInPlanned()
    {
        var item = DefaultAccessBadgeBundle.NewDefaultAccessBadgeBundle("code-94", "name-212");
        Check.Equal(item.Status, DefaultAccessBadgeBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAccessBadgeBundleMovesToInProgress()
    {
        var item = DefaultAccessBadgeBundle.NewDefaultAccessBadgeBundle("code-94", "name-212");
        try
        {
            item.MoveTo(DefaultAccessBadgeBundleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAccessBadgeBundleStatus.InProgress, "status");
    }

    public static void DefaultAccessBadgeBundleRejectsMovesFromCancelled()
    {
        var item = DefaultAccessBadgeBundle.NewDefaultAccessBadgeBundle("code-94", "name-212");
        item.Status = DefaultAccessBadgeBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultAccessBadgeBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeBundleStartsInPlanned();
        DefaultAccessBadgeBundleMovesToInProgress();
        DefaultAccessBadgeBundleRejectsMovesFromCancelled();
    }
}
