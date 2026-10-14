namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class OccupancyTierChecks
{
    public static void OccupancyTierStartsInCreated()
    {
        var item = OccupancyTier.NewOccupancyTier("code-633", "title-778");
        Check.Equal(item.Status, OccupancyTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyTierMovesToValidated()
    {
        var item = OccupancyTier.NewOccupancyTier("code-633", "title-778");
        try
        {
            item.MoveTo(OccupancyTierStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyTierStatus.Validated, "status");
    }

    public static void OccupancyTierRejectsMovesFromRetired()
    {
        var item = OccupancyTier.NewOccupancyTier("code-633", "title-778");
        item.Status = OccupancyTierStatus.Retired;
        Check.Fails(() => item.MoveTo(OccupancyTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        OccupancyTierStartsInCreated();
        OccupancyTierMovesToValidated();
        OccupancyTierRejectsMovesFromRetired();
    }
}
