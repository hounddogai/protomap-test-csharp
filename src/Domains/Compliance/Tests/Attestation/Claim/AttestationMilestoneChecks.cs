namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationMilestoneChecks
{
    public static void AttestationMilestoneStartsInPlanned()
    {
        var item = AttestationMilestone.NewAttestationMilestone("code-202", "label-844");
        Check.Equal(item.Status, AttestationMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationMilestoneMovesToScheduled()
    {
        var item = AttestationMilestone.NewAttestationMilestone("code-202", "label-844");
        try
        {
            item.MoveTo(AttestationMilestoneStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationMilestoneStatus.Scheduled, "status");
    }

    public static void AttestationMilestoneRejectsMovesFromCancelled()
    {
        var item = AttestationMilestone.NewAttestationMilestone("code-202", "label-844");
        item.Status = AttestationMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AttestationMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AttestationMilestoneStartsInPlanned();
        AttestationMilestoneMovesToScheduled();
        AttestationMilestoneRejectsMovesFromCancelled();
    }
}
