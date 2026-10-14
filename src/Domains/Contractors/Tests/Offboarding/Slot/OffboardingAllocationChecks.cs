namespace Acme.Workforce.Contractors.Tests.Offboarding.Slot;

using Acme.Workforce.Contractors.Offboarding.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class OffboardingAllocationChecks
{
    public static void OffboardingAllocationStartsInProposed()
    {
        var item = OffboardingAllocation.NewOffboardingAllocation("code-143", "name-13");
        Check.Equal(item.Status, OffboardingAllocationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingAllocationMovesToRejected()
    {
        var item = OffboardingAllocation.NewOffboardingAllocation("code-143", "name-13");
        try
        {
            item.MoveTo(OffboardingAllocationStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingAllocationStatus.Rejected, "status");
    }

    public static void OffboardingAllocationRejectsMovesFromWithdrawn()
    {
        var item = OffboardingAllocation.NewOffboardingAllocation("code-143", "name-13");
        item.Status = OffboardingAllocationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(OffboardingAllocationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        OffboardingAllocationStartsInProposed();
        OffboardingAllocationMovesToRejected();
        OffboardingAllocationRejectsMovesFromWithdrawn();
    }
}
