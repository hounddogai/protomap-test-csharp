namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class DefaultBonusPoolCheckpointChecks
{
    public static void DefaultBonusPoolCheckpointStartsInDraft()
    {
        var item = DefaultBonusPoolCheckpoint.NewDefaultBonusPoolCheckpoint("code-611", "name-858");
        Check.Equal(item.Status, DefaultBonusPoolCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultBonusPoolCheckpointMovesToSuspended()
    {
        var item = DefaultBonusPoolCheckpoint.NewDefaultBonusPoolCheckpoint("code-611", "name-858");
        try
        {
            item.MoveTo(DefaultBonusPoolCheckpointStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultBonusPoolCheckpointStatus.Suspended, "status");
    }

    public static void DefaultBonusPoolCheckpointRejectsMovesFromClosed()
    {
        var item = DefaultBonusPoolCheckpoint.NewDefaultBonusPoolCheckpoint("code-611", "name-858");
        item.Status = DefaultBonusPoolCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultBonusPoolCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultBonusPoolCheckpointStartsInDraft();
        DefaultBonusPoolCheckpointMovesToSuspended();
        DefaultBonusPoolCheckpointRejectsMovesFromClosed();
    }
}
