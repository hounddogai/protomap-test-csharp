namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackMeritCycleCheckpointChecks
{
    public static void FallbackMeritCycleCheckpointStartsInDraft()
    {
        var item = FallbackMeritCycleCheckpoint.NewFallbackMeritCycleCheckpoint("code-89", "name-799");
        Check.Equal(item.Status, FallbackMeritCycleCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackMeritCycleCheckpointMovesToPending()
    {
        var item = FallbackMeritCycleCheckpoint.NewFallbackMeritCycleCheckpoint("code-89", "name-799");
        try
        {
            item.MoveTo(FallbackMeritCycleCheckpointStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackMeritCycleCheckpointStatus.Pending, "status");
    }

    public static void FallbackMeritCycleCheckpointRejectsMovesFromClosed()
    {
        var item = FallbackMeritCycleCheckpoint.NewFallbackMeritCycleCheckpoint("code-89", "name-799");
        item.Status = FallbackMeritCycleCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackMeritCycleCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackMeritCycleCheckpointStartsInDraft();
        FallbackMeritCycleCheckpointMovesToPending();
        FallbackMeritCycleCheckpointRejectsMovesFromClosed();
    }
}
