namespace Acme.Workforce.Compliance.Tests.Attestation.Milestone;

using Acme.Workforce.Compliance.Attestation.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AdaptiveAttestationMilestoneChecks
{
    public static void AdaptiveAttestationMilestoneStartsInPlanned()
    {
        var item = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone("code-332", "name-331");
        Check.Equal(item.Status, AdaptiveAttestationMilestoneStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAttestationMilestoneMovesToCompleted()
    {
        var item = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone("code-332", "name-331");
        try
        {
            item.MoveTo(AdaptiveAttestationMilestoneStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAttestationMilestoneStatus.Completed, "status");
    }

    public static void AdaptiveAttestationMilestoneRejectsMovesFromCancelled()
    {
        var item = AdaptiveAttestationMilestone.NewAdaptiveAttestationMilestone("code-332", "name-331");
        item.Status = AdaptiveAttestationMilestoneStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveAttestationMilestoneStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveAttestationMilestoneStartsInPlanned();
        AdaptiveAttestationMilestoneMovesToCompleted();
        AdaptiveAttestationMilestoneRejectsMovesFromCancelled();
    }
}
