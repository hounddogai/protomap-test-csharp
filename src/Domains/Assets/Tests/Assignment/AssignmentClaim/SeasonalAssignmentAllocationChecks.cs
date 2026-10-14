namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentClaim;

using Acme.Workforce.Assets.Assignment.AssignmentClaim;

/// <summary>Checks of assignment claim code, run by the project's runner.</summary>
public static class SeasonalAssignmentAllocationChecks
{
    public static void SeasonalAssignmentAllocationStartsInDraft()
    {
        var item = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation("code-641", "name-40");
        Check.Equal(item.Status, SeasonalAssignmentAllocationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAssignmentAllocationMovesToPending()
    {
        var item = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation("code-641", "name-40");
        try
        {
            item.MoveTo(SeasonalAssignmentAllocationStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAssignmentAllocationStatus.Pending, "status");
    }

    public static void SeasonalAssignmentAllocationRejectsMovesFromClosed()
    {
        var item = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation("code-641", "name-40");
        item.Status = SeasonalAssignmentAllocationStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalAssignmentAllocationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalAssignmentAllocationStartsInDraft();
        SeasonalAssignmentAllocationMovesToPending();
        SeasonalAssignmentAllocationRejectsMovesFromClosed();
    }
}
