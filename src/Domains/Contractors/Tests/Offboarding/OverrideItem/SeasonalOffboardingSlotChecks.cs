namespace Acme.Workforce.Contractors.Tests.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class SeasonalOffboardingSlotChecks
{
    public static void SeasonalOffboardingSlotStartsInIdle()
    {
        var item = SeasonalOffboardingSlot.NewSeasonalOffboardingSlot("code-506", "name-254");
        Check.Equal(item.Status, SeasonalOffboardingSlotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalOffboardingSlotMovesToWarming()
    {
        var item = SeasonalOffboardingSlot.NewSeasonalOffboardingSlot("code-506", "name-254");
        try
        {
            item.MoveTo(SeasonalOffboardingSlotStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalOffboardingSlotStatus.Warming, "status");
    }

    public static void SeasonalOffboardingSlotRejectsMovesFromStopped()
    {
        var item = SeasonalOffboardingSlot.NewSeasonalOffboardingSlot("code-506", "name-254");
        item.Status = SeasonalOffboardingSlotStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalOffboardingSlotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalOffboardingSlotStartsInIdle();
        SeasonalOffboardingSlotMovesToWarming();
        SeasonalOffboardingSlotRejectsMovesFromStopped();
    }
}
