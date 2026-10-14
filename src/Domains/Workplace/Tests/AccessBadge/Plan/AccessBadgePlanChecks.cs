namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AccessBadgePlanChecks
{
    public static void AccessBadgePlanStartsInDraft()
    {
        var item = AccessBadgePlan.NewAccessBadgePlan("code-720", "name-277");
        Check.Equal(item.Status, AccessBadgePlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgePlanMovesToPending()
    {
        var item = AccessBadgePlan.NewAccessBadgePlan("code-720", "name-277");
        try
        {
            item.MoveTo(AccessBadgePlanStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgePlanStatus.Pending, "status");
    }

    public static void AccessBadgePlanRejectsMovesFromClosed()
    {
        var item = AccessBadgePlan.NewAccessBadgePlan("code-720", "name-277");
        item.Status = AccessBadgePlanStatus.Closed;
        Check.Fails(() => item.MoveTo(AccessBadgePlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccessBadgePlanStartsInDraft();
        AccessBadgePlanMovesToPending();
        AccessBadgePlanRejectsMovesFromClosed();
    }
}
