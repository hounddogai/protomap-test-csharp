namespace Acme.Workforce.Compliance.Tests.Certification.Adjustment;

using Acme.Workforce.Compliance.Certification.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WeightedCertificationMilestoneChecks
{
    public static void WeightedCertificationMilestoneStartsInProposed()
    {
        var item = WeightedCertificationMilestone.NewWeightedCertificationMilestone("code-347", "name-531");
        Check.Equal(item.Status, WeightedCertificationMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedCertificationMilestoneMovesToApproved()
    {
        var item = WeightedCertificationMilestone.NewWeightedCertificationMilestone("code-347", "name-531");
        try
        {
            item.MoveTo(WeightedCertificationMilestoneStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedCertificationMilestoneStatus.Approved, "status");
    }

    public static void WeightedCertificationMilestoneRejectsMovesFromWithdrawn()
    {
        var item = WeightedCertificationMilestone.NewWeightedCertificationMilestone("code-347", "name-531");
        item.Status = WeightedCertificationMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedCertificationMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedCertificationMilestoneStartsInProposed();
        WeightedCertificationMilestoneMovesToApproved();
        WeightedCertificationMilestoneRejectsMovesFromWithdrawn();
    }
}
