namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyGroupChecks
{
    public static void OccupancyGroupStartsInCreated()
    {
        var item = OccupancyGroup.NewOccupancyGroup("code-615", "name-128");
        Check.Equal(item.Status, OccupancyGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyGroupMovesToValidated()
    {
        var item = OccupancyGroup.NewOccupancyGroup("code-615", "name-128");
        try
        {
            item.MoveTo(OccupancyGroupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyGroupStatus.Validated, "status");
    }

    public static void OccupancyGroupRejectsMovesFromRetired()
    {
        var item = OccupancyGroup.NewOccupancyGroup("code-615", "name-128");
        item.Status = OccupancyGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(OccupancyGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        OccupancyGroupStartsInCreated();
        OccupancyGroupMovesToValidated();
        OccupancyGroupRejectsMovesFromRetired();
    }
}
