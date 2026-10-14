namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathEntryChecks
{
    public static void CareerPathEntryStartsInRequested()
    {
        var item = CareerPathEntry.NewCareerPathEntry("code-798", "name-60");
        Check.Equal(item.Status, CareerPathEntryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathEntryMovesToGranted()
    {
        var item = CareerPathEntry.NewCareerPathEntry("code-798", "name-60");
        try
        {
            item.MoveTo(CareerPathEntryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathEntryStatus.Granted, "status");
    }

    public static void CareerPathEntryRejectsMovesFromExpired()
    {
        var item = CareerPathEntry.NewCareerPathEntry("code-798", "name-60");
        item.Status = CareerPathEntryStatus.Expired;
        Check.Fails(() => item.MoveTo(CareerPathEntryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CareerPathEntryStartsInRequested();
        CareerPathEntryMovesToGranted();
        CareerPathEntryRejectsMovesFromExpired();
    }
}
