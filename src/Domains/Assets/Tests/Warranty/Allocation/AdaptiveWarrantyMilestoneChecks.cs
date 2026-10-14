namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdaptiveWarrantyMilestoneChecks
{
    public static void AdaptiveWarrantyMilestoneStartsInProposed()
    {
        var item = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone("code-140", "label-164");
        Check.Equal(item.Status, AdaptiveWarrantyMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveWarrantyMilestoneMovesToApproved()
    {
        var item = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone("code-140", "label-164");
        try
        {
            item.MoveTo(AdaptiveWarrantyMilestoneStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveWarrantyMilestoneStatus.Approved, "status");
    }

    public static void AdaptiveWarrantyMilestoneRejectsMovesFromWithdrawn()
    {
        var item = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone("code-140", "label-164");
        item.Status = AdaptiveWarrantyMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdaptiveWarrantyMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyMilestoneStartsInProposed();
        AdaptiveWarrantyMilestoneMovesToApproved();
        AdaptiveWarrantyMilestoneRejectsMovesFromWithdrawn();
    }
}
