namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleCheckpointChecks
{
    public static void MeritCycleCheckpointStartsInProposed()
    {
        var item = MeritCycleCheckpoint.NewMeritCycleCheckpoint("code-607", "name-46");
        Check.Equal(item.Status, MeritCycleCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleCheckpointMovesToApproved()
    {
        var item = MeritCycleCheckpoint.NewMeritCycleCheckpoint("code-607", "name-46");
        try
        {
            item.MoveTo(MeritCycleCheckpointStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleCheckpointStatus.Approved, "status");
    }

    public static void MeritCycleCheckpointRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleCheckpoint.NewMeritCycleCheckpoint("code-607", "name-46");
        item.Status = MeritCycleCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleCheckpointStartsInProposed();
        MeritCycleCheckpointMovesToApproved();
        MeritCycleCheckpointRejectsMovesFromWithdrawn();
    }
}
