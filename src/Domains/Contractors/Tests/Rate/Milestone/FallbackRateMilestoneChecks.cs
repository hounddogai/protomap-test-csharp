namespace Acme.Workforce.Contractors.Tests.Rate.Milestone;

using Acme.Workforce.Contractors.Rate.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class FallbackRateMilestoneChecks
{
    public static void FallbackRateMilestoneStartsInProposed()
    {
        var item = FallbackRateMilestone.NewFallbackRateMilestone("code-443", "name-30");
        Check.Equal(item.Status, FallbackRateMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackRateMilestoneMovesToApproved()
    {
        var item = FallbackRateMilestone.NewFallbackRateMilestone("code-443", "name-30");
        try
        {
            item.MoveTo(FallbackRateMilestoneStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackRateMilestoneStatus.Approved, "status");
    }

    public static void FallbackRateMilestoneRejectsMovesFromWithdrawn()
    {
        var item = FallbackRateMilestone.NewFallbackRateMilestone("code-443", "name-30");
        item.Status = FallbackRateMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackRateMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackRateMilestoneStartsInProposed();
        FallbackRateMilestoneMovesToApproved();
        FallbackRateMilestoneRejectsMovesFromWithdrawn();
    }
}
