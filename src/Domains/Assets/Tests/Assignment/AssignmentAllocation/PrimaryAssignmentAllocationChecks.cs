namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentAllocation;

using Acme.Workforce.Assets.Assignment.AssignmentAllocation;

/// <summary>Checks of assignment allocation code, run by the project's runner.</summary>
public static class PrimaryAssignmentAllocationChecks
{
    public static void PrimaryAssignmentAllocationStartsInRequested()
    {
        var item = PrimaryAssignmentAllocation.NewPrimaryAssignmentAllocation("code-224", "name-88");
        Check.Equal(item.Status, PrimaryAssignmentAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAssignmentAllocationMovesToRevoked()
    {
        var item = PrimaryAssignmentAllocation.NewPrimaryAssignmentAllocation("code-224", "name-88");
        try
        {
            item.MoveTo(PrimaryAssignmentAllocationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAssignmentAllocationStatus.Revoked, "status");
    }

    public static void PrimaryAssignmentAllocationRejectsMovesFromExpired()
    {
        var item = PrimaryAssignmentAllocation.NewPrimaryAssignmentAllocation("code-224", "name-88");
        item.Status = PrimaryAssignmentAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryAssignmentAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryAssignmentAllocationStartsInRequested();
        PrimaryAssignmentAllocationMovesToRevoked();
        PrimaryAssignmentAllocationRejectsMovesFromExpired();
    }
}
