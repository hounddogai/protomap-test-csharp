namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class DefaultAccessBadgeRollupChecks
{
    public static void DefaultAccessBadgeRollupStartsInDraft()
    {
        var item = DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup("code-16", "label-466");
        Check.Equal(item.Status, DefaultAccessBadgeRollupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAccessBadgeRollupMovesToClosed()
    {
        var item = DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup("code-16", "label-466");
        try
        {
            item.MoveTo(DefaultAccessBadgeRollupStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAccessBadgeRollupStatus.Closed, "status");
    }

    public static void DefaultAccessBadgeRollupRejectsMovesFromClosed()
    {
        var item = DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup("code-16", "label-466");
        item.Status = DefaultAccessBadgeRollupStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultAccessBadgeRollupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeRollupStartsInDraft();
        DefaultAccessBadgeRollupMovesToClosed();
        DefaultAccessBadgeRollupRejectsMovesFromClosed();
    }
}
