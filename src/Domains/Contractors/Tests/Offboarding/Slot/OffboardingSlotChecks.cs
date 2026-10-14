namespace Acme.Workforce.Contractors.Tests.Offboarding.Slot;

using Acme.Workforce.Contractors.Offboarding.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class OffboardingSlotChecks
{
    public static void OffboardingSlotStartsInCreated()
    {
        var item = OffboardingSlot.NewOffboardingSlot("code-730", "name-431");
        Check.Equal(item.Status, OffboardingSlotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingSlotMovesToValidated()
    {
        var item = OffboardingSlot.NewOffboardingSlot("code-730", "name-431");
        try
        {
            item.MoveTo(OffboardingSlotStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingSlotStatus.Validated, "status");
    }

    public static void OffboardingSlotRejectsMovesFromRetired()
    {
        var item = OffboardingSlot.NewOffboardingSlot("code-730", "name-431");
        item.Status = OffboardingSlotStatus.Retired;
        Check.Fails(() => item.MoveTo(OffboardingSlotStatus.Created), "move");
    }

    public static void RunAll()
    {
        OffboardingSlotStartsInCreated();
        OffboardingSlotMovesToValidated();
        OffboardingSlotRejectsMovesFromRetired();
    }
}
