namespace Acme.Workforce.Talent.Tests.CareerPath.Slot;

using Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CareerPathSlotChecks
{
    public static void CareerPathSlotStartsInRequested()
    {
        var item = CareerPathSlot.NewCareerPathSlot("code-328", "title-716");
        Check.Equal(item.Status, CareerPathSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathSlotMovesToGranted()
    {
        var item = CareerPathSlot.NewCareerPathSlot("code-328", "title-716");
        try
        {
            item.MoveTo(CareerPathSlotStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathSlotStatus.Granted, "status");
    }

    public static void CareerPathSlotRejectsMovesFromExpired()
    {
        var item = CareerPathSlot.NewCareerPathSlot("code-328", "title-716");
        item.Status = CareerPathSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(CareerPathSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CareerPathSlotStartsInRequested();
        CareerPathSlotMovesToGranted();
        CareerPathSlotRejectsMovesFromExpired();
    }
}
