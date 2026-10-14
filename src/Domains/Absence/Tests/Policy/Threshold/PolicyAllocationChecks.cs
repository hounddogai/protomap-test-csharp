namespace Acme.Workforce.Absence.Tests.Policy.Threshold;

using Acme.Workforce.Absence.Policy.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PolicyAllocationChecks
{
    public static void PolicyAllocationStartsInRequested()
    {
        var item = PolicyAllocation.NewPolicyAllocation("code-111", "label-447");
        Check.Equal(item.Status, PolicyAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAllocationMovesToGranted()
    {
        var item = PolicyAllocation.NewPolicyAllocation("code-111", "label-447");
        try
        {
            item.MoveTo(PolicyAllocationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAllocationStatus.Granted, "status");
    }

    public static void PolicyAllocationRejectsMovesFromExpired()
    {
        var item = PolicyAllocation.NewPolicyAllocation("code-111", "label-447");
        item.Status = PolicyAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(PolicyAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PolicyAllocationStartsInRequested();
        PolicyAllocationMovesToGranted();
        PolicyAllocationRejectsMovesFromExpired();
    }
}
