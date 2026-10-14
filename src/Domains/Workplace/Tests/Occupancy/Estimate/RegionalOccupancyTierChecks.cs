namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RegionalOccupancyTierChecks
{
    public static void RegionalOccupancyTierStartsInCreated()
    {
        var item = RegionalOccupancyTier.NewRegionalOccupancyTier("code-668", "name-299");
        Check.Equal(item.Status, RegionalOccupancyTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalOccupancyTierMovesToValidated()
    {
        var item = RegionalOccupancyTier.NewRegionalOccupancyTier("code-668", "name-299");
        try
        {
            item.MoveTo(RegionalOccupancyTierStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalOccupancyTierStatus.Validated, "status");
    }

    public static void RegionalOccupancyTierRejectsMovesFromRetired()
    {
        var item = RegionalOccupancyTier.NewRegionalOccupancyTier("code-668", "name-299");
        item.Status = RegionalOccupancyTierStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalOccupancyTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalOccupancyTierStartsInCreated();
        RegionalOccupancyTierMovesToValidated();
        RegionalOccupancyTierRejectsMovesFromRetired();
    }
}
