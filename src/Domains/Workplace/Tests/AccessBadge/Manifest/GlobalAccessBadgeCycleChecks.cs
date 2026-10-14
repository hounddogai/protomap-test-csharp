namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class GlobalAccessBadgeCycleChecks
{
    public static void GlobalAccessBadgeCycleStartsInProposed()
    {
        var item = GlobalAccessBadgeCycle.NewGlobalAccessBadgeCycle("code-121", "title-882");
        Check.Equal(item.Status, GlobalAccessBadgeCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAccessBadgeCycleMovesToRejected()
    {
        var item = GlobalAccessBadgeCycle.NewGlobalAccessBadgeCycle("code-121", "title-882");
        try
        {
            item.MoveTo(GlobalAccessBadgeCycleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAccessBadgeCycleStatus.Rejected, "status");
    }

    public static void GlobalAccessBadgeCycleRejectsMovesFromWithdrawn()
    {
        var item = GlobalAccessBadgeCycle.NewGlobalAccessBadgeCycle("code-121", "title-882");
        item.Status = GlobalAccessBadgeCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GlobalAccessBadgeCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GlobalAccessBadgeCycleStartsInProposed();
        GlobalAccessBadgeCycleMovesToRejected();
        GlobalAccessBadgeCycleRejectsMovesFromWithdrawn();
    }
}
