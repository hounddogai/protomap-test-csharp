namespace Acme.Workforce.Assets.Tests.Laptop.Rollup;

using Acme.Workforce.Assets.Laptop.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedLaptopRollupChecks
{
    public static void WeightedLaptopRollupStartsInCreated()
    {
        var item = WeightedLaptopRollup.NewWeightedLaptopRollup("code-260", "name-745");
        Check.Equal(item.Status, WeightedLaptopRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedLaptopRollupMovesToValidated()
    {
        var item = WeightedLaptopRollup.NewWeightedLaptopRollup("code-260", "name-745");
        try
        {
            item.MoveTo(WeightedLaptopRollupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedLaptopRollupStatus.Validated, "status");
    }

    public static void WeightedLaptopRollupRejectsMovesFromRetired()
    {
        var item = WeightedLaptopRollup.NewWeightedLaptopRollup("code-260", "name-745");
        item.Status = WeightedLaptopRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedLaptopRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedLaptopRollupStartsInCreated();
        WeightedLaptopRollupMovesToValidated();
        WeightedLaptopRollupRejectsMovesFromRetired();
    }
}
