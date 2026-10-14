namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>Checks of offboarding threshold code, run by the project's runner.</summary>
public static class PrimaryOffboardingSlotChecks
{
    public static void PrimaryOffboardingSlotStartsInRequested()
    {
        var item = PrimaryOffboardingSlot.NewPrimaryOffboardingSlot("code-416", "name-182");
        Check.Equal(item.Status, PrimaryOffboardingSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryOffboardingSlotMovesToExpired()
    {
        var item = PrimaryOffboardingSlot.NewPrimaryOffboardingSlot("code-416", "name-182");
        try
        {
            item.MoveTo(PrimaryOffboardingSlotStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryOffboardingSlotStatus.Expired, "status");
    }

    public static void PrimaryOffboardingSlotRejectsMovesFromExpired()
    {
        var item = PrimaryOffboardingSlot.NewPrimaryOffboardingSlot("code-416", "name-182");
        item.Status = PrimaryOffboardingSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryOffboardingSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryOffboardingSlotStartsInRequested();
        PrimaryOffboardingSlotMovesToExpired();
        PrimaryOffboardingSlotRejectsMovesFromExpired();
    }
}
