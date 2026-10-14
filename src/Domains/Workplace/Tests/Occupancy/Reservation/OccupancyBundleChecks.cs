namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyBundleChecks
{
    public static void OccupancyBundleStartsInRequested()
    {
        var item = OccupancyBundle.NewOccupancyBundle("code-138", "title-218");
        Check.Equal(item.Status, OccupancyBundleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyBundleMovesToGranted()
    {
        var item = OccupancyBundle.NewOccupancyBundle("code-138", "title-218");
        try
        {
            item.MoveTo(OccupancyBundleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyBundleStatus.Granted, "status");
    }

    public static void OccupancyBundleRejectsMovesFromExpired()
    {
        var item = OccupancyBundle.NewOccupancyBundle("code-138", "title-218");
        item.Status = OccupancyBundleStatus.Expired;
        Check.Fails(() => item.MoveTo(OccupancyBundleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        OccupancyBundleStartsInRequested();
        OccupancyBundleMovesToGranted();
        OccupancyBundleRejectsMovesFromExpired();
    }
}
