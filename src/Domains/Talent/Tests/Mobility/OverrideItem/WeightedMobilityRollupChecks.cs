namespace Acme.Workforce.Talent.Tests.Mobility.OverrideItem;

using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class WeightedMobilityRollupChecks
{
    public static void WeightedMobilityRollupStartsInCreated()
    {
        var item = WeightedMobilityRollup.NewWeightedMobilityRollup("code-599", "name-549");
        Check.Equal(item.Status, WeightedMobilityRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedMobilityRollupMovesToRetired()
    {
        var item = WeightedMobilityRollup.NewWeightedMobilityRollup("code-599", "name-549");
        try
        {
            item.MoveTo(WeightedMobilityRollupStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedMobilityRollupStatus.Retired, "status");
    }

    public static void WeightedMobilityRollupRejectsMovesFromRetired()
    {
        var item = WeightedMobilityRollup.NewWeightedMobilityRollup("code-599", "name-549");
        item.Status = WeightedMobilityRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedMobilityRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedMobilityRollupStartsInCreated();
        WeightedMobilityRollupMovesToRetired();
        WeightedMobilityRollupRejectsMovesFromRetired();
    }
}
