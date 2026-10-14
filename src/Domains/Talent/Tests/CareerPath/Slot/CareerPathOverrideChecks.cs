namespace Acme.Workforce.Talent.Tests.CareerPath.Slot;

using Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CareerPathOverrideChecks
{
    public static void CareerPathOverrideStartsInRequested()
    {
        var item = CareerPathOverride.NewCareerPathOverride("code-221", "name-787");
        Check.Equal(item.Status, CareerPathOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathOverrideMovesToGranted()
    {
        var item = CareerPathOverride.NewCareerPathOverride("code-221", "name-787");
        try
        {
            item.MoveTo(CareerPathOverrideStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathOverrideStatus.Granted, "status");
    }

    public static void CareerPathOverrideRejectsMovesFromExpired()
    {
        var item = CareerPathOverride.NewCareerPathOverride("code-221", "name-787");
        item.Status = CareerPathOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(CareerPathOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CareerPathOverrideStartsInRequested();
        CareerPathOverrideMovesToGranted();
        CareerPathOverrideRejectsMovesFromExpired();
    }
}
