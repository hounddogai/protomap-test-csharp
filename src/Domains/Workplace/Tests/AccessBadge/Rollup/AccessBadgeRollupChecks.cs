namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeRollupChecks
{
    public static void AccessBadgeRollupStartsInDraft()
    {
        var item = AccessBadgeRollup.NewAccessBadgeRollup("code-717", "label-442");
        Check.Equal(item.Status, AccessBadgeRollupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeRollupMovesToActive()
    {
        var item = AccessBadgeRollup.NewAccessBadgeRollup("code-717", "label-442");
        try
        {
            item.MoveTo(AccessBadgeRollupStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeRollupStatus.Active, "status");
    }

    public static void AccessBadgeRollupRejectsMovesFromClosed()
    {
        var item = AccessBadgeRollup.NewAccessBadgeRollup("code-717", "label-442");
        item.Status = AccessBadgeRollupStatus.Closed;
        Check.Fails(() => item.MoveTo(AccessBadgeRollupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccessBadgeRollupStartsInDraft();
        AccessBadgeRollupMovesToActive();
        AccessBadgeRollupRejectsMovesFromClosed();
    }
}
