namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

/// <summary>Checks of offboarding slot code, run by the project's runner.</summary>
public static class RegionalOffboardingSlotChecks
{
    public static void RegionalOffboardingSlotStartsInProposed()
    {
        var item = RegionalOffboardingSlot.NewRegionalOffboardingSlot("code-89", "name-527");
        Check.Equal(item.Status, RegionalOffboardingSlotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalOffboardingSlotMovesToApproved()
    {
        var item = RegionalOffboardingSlot.NewRegionalOffboardingSlot("code-89", "name-527");
        try
        {
            item.MoveTo(RegionalOffboardingSlotStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalOffboardingSlotStatus.Approved, "status");
    }

    public static void RegionalOffboardingSlotRejectsMovesFromWithdrawn()
    {
        var item = RegionalOffboardingSlot.NewRegionalOffboardingSlot("code-89", "name-527");
        item.Status = RegionalOffboardingSlotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalOffboardingSlotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalOffboardingSlotStartsInProposed();
        RegionalOffboardingSlotMovesToApproved();
        RegionalOffboardingSlotRejectsMovesFromWithdrawn();
    }
}
