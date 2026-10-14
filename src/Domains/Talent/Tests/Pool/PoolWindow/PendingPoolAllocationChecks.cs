namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class PendingPoolAllocationChecks
{
    public static void PendingPoolAllocationStartsInCreated()
    {
        var item = PendingPoolAllocation.NewPendingPoolAllocation("code-485", "title-634");
        Check.Equal(item.Status, PendingPoolAllocationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPoolAllocationMovesToValidated()
    {
        var item = PendingPoolAllocation.NewPendingPoolAllocation("code-485", "title-634");
        try
        {
            item.MoveTo(PendingPoolAllocationStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPoolAllocationStatus.Validated, "status");
    }

    public static void PendingPoolAllocationRejectsMovesFromRetired()
    {
        var item = PendingPoolAllocation.NewPendingPoolAllocation("code-485", "title-634");
        item.Status = PendingPoolAllocationStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingPoolAllocationStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingPoolAllocationStartsInCreated();
        PendingPoolAllocationMovesToValidated();
        PendingPoolAllocationRejectsMovesFromRetired();
    }
}
