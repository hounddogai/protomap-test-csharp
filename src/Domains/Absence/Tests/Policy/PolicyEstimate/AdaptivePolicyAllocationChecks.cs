namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class AdaptivePolicyAllocationChecks
{
    public static void AdaptivePolicyAllocationStartsInRequested()
    {
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("code-759", "label-649");
        Check.Equal(item.Status, AdaptivePolicyAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePolicyAllocationMovesToExpired()
    {
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("code-759", "label-649");
        try
        {
            item.MoveTo(AdaptivePolicyAllocationStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePolicyAllocationStatus.Expired, "status");
    }

    public static void AdaptivePolicyAllocationRejectsMovesFromExpired()
    {
        var item = AdaptivePolicyAllocation.NewAdaptivePolicyAllocation("code-759", "label-649");
        item.Status = AdaptivePolicyAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptivePolicyAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptivePolicyAllocationStartsInRequested();
        AdaptivePolicyAllocationMovesToExpired();
        AdaptivePolicyAllocationRejectsMovesFromExpired();
    }
}
