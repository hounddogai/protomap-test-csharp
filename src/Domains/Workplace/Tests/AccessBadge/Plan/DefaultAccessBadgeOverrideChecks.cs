namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultAccessBadgeOverrideChecks
{
    public static void DefaultAccessBadgeOverrideStartsInProposed()
    {
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("code-529", "title-767");
        Check.Equal(item.Status, DefaultAccessBadgeOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAccessBadgeOverrideMovesToApproved()
    {
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("code-529", "title-767");
        try
        {
            item.MoveTo(DefaultAccessBadgeOverrideStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAccessBadgeOverrideStatus.Approved, "status");
    }

    public static void DefaultAccessBadgeOverrideRejectsMovesFromWithdrawn()
    {
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("code-529", "title-767");
        item.Status = DefaultAccessBadgeOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultAccessBadgeOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeOverrideStartsInProposed();
        DefaultAccessBadgeOverrideMovesToApproved();
        DefaultAccessBadgeOverrideRejectsMovesFromWithdrawn();
    }
}
