namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class GlobalPolicyAllocationChecks
{
    public static void GlobalPolicyAllocationStartsInIdle()
    {
        var item = GlobalPolicyAllocation.NewGlobalPolicyAllocation("code-497", "name-777");
        Check.Equal(item.Status, GlobalPolicyAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPolicyAllocationMovesToWarming()
    {
        var item = GlobalPolicyAllocation.NewGlobalPolicyAllocation("code-497", "name-777");
        try
        {
            item.MoveTo(GlobalPolicyAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPolicyAllocationStatus.Warming, "status");
    }

    public static void GlobalPolicyAllocationRejectsMovesFromStopped()
    {
        var item = GlobalPolicyAllocation.NewGlobalPolicyAllocation("code-497", "name-777");
        item.Status = GlobalPolicyAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalPolicyAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalPolicyAllocationStartsInIdle();
        GlobalPolicyAllocationMovesToWarming();
        GlobalPolicyAllocationRejectsMovesFromStopped();
    }
}
