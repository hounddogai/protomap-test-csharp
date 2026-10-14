namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeCheckpointChecks
{
    public static void AccessBadgeCheckpointStartsInNew()
    {
        var item = AccessBadgeCheckpoint.NewAccessBadgeCheckpoint("code-208", "name-308");
        Check.Equal(item.Status, AccessBadgeCheckpointStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeCheckpointMovesToQueued()
    {
        var item = AccessBadgeCheckpoint.NewAccessBadgeCheckpoint("code-208", "name-308");
        try
        {
            item.MoveTo(AccessBadgeCheckpointStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeCheckpointStatus.Queued, "status");
    }

    public static void AccessBadgeCheckpointRejectsMovesFromFailed()
    {
        var item = AccessBadgeCheckpoint.NewAccessBadgeCheckpoint("code-208", "name-308");
        item.Status = AccessBadgeCheckpointStatus.Failed;
        Check.Fails(() => item.MoveTo(AccessBadgeCheckpointStatus.New), "move");
    }

    public static void RunAll()
    {
        AccessBadgeCheckpointStartsInNew();
        AccessBadgeCheckpointMovesToQueued();
        AccessBadgeCheckpointRejectsMovesFromFailed();
    }
}
