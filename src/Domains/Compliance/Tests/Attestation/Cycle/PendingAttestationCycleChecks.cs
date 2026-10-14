namespace Acme.Workforce.Compliance.Tests.Attestation.Cycle;

using Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PendingAttestationCycleChecks
{
    public static void PendingAttestationCycleStartsInPlanned()
    {
        var item = PendingAttestationCycle.NewPendingAttestationCycle("code-664", "name-938");
        Check.Equal(item.Status, PendingAttestationCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAttestationCycleMovesToScheduled()
    {
        var item = PendingAttestationCycle.NewPendingAttestationCycle("code-664", "name-938");
        try
        {
            item.MoveTo(PendingAttestationCycleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAttestationCycleStatus.Scheduled, "status");
    }

    public static void PendingAttestationCycleRejectsMovesFromCancelled()
    {
        var item = PendingAttestationCycle.NewPendingAttestationCycle("code-664", "name-938");
        item.Status = PendingAttestationCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingAttestationCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingAttestationCycleStartsInPlanned();
        PendingAttestationCycleMovesToScheduled();
        PendingAttestationCycleRejectsMovesFromCancelled();
    }
}
