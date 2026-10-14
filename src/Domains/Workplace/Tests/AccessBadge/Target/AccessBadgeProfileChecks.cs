namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeProfileChecks
{
    public static void AccessBadgeProfileStartsInDraft()
    {
        var item = AccessBadgeProfile.NewAccessBadgeProfile("code-224", "name-889");
        Check.Equal(item.Status, AccessBadgeProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeProfileMovesToPending()
    {
        var item = AccessBadgeProfile.NewAccessBadgeProfile("code-224", "name-889");
        try
        {
            item.MoveTo(AccessBadgeProfileStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeProfileStatus.Pending, "status");
    }

    public static void AccessBadgeProfileRejectsMovesFromClosed()
    {
        var item = AccessBadgeProfile.NewAccessBadgeProfile("code-224", "name-889");
        item.Status = AccessBadgeProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(AccessBadgeProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccessBadgeProfileStartsInDraft();
        AccessBadgeProfileMovesToPending();
        AccessBadgeProfileRejectsMovesFromClosed();
    }
}
