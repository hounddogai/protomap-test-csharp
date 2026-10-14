namespace Acme.Workforce.Workplace.Tests.AccessBadge.Budget;

using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AccessBadgeAdjustmentChecks
{
    public static void AccessBadgeAdjustmentStartsInIdle()
    {
        var item = AccessBadgeAdjustment.NewAccessBadgeAdjustment("code-77", "title-805");
        Check.Equal(item.Status, AccessBadgeAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeAdjustmentMovesToWarming()
    {
        var item = AccessBadgeAdjustment.NewAccessBadgeAdjustment("code-77", "title-805");
        try
        {
            item.MoveTo(AccessBadgeAdjustmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeAdjustmentStatus.Warming, "status");
    }

    public static void AccessBadgeAdjustmentRejectsMovesFromStopped()
    {
        var item = AccessBadgeAdjustment.NewAccessBadgeAdjustment("code-77", "title-805");
        item.Status = AccessBadgeAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(AccessBadgeAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AccessBadgeAdjustmentStartsInIdle();
        AccessBadgeAdjustmentMovesToWarming();
        AccessBadgeAdjustmentRejectsMovesFromStopped();
    }
}
