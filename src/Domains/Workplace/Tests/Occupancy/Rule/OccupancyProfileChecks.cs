namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class OccupancyProfileChecks
{
    public static void OccupancyProfileStartsInIdle()
    {
        var item = OccupancyProfile.NewOccupancyProfile("code-289", "title-856");
        Check.Equal(item.Status, OccupancyProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyProfileMovesToWarming()
    {
        var item = OccupancyProfile.NewOccupancyProfile("code-289", "title-856");
        try
        {
            item.MoveTo(OccupancyProfileStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyProfileStatus.Warming, "status");
    }

    public static void OccupancyProfileRejectsMovesFromStopped()
    {
        var item = OccupancyProfile.NewOccupancyProfile("code-289", "title-856");
        item.Status = OccupancyProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(OccupancyProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        OccupancyProfileStartsInIdle();
        OccupancyProfileMovesToWarming();
        OccupancyProfileRejectsMovesFromStopped();
    }
}
