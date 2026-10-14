namespace Acme.Workforce.Assets.Tests.Laptop.Target;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class WeightedLaptopEstimateChecks
{
    public static void WeightedLaptopEstimateStartsInNew()
    {
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("code-152", "title-599");
        Check.Equal(item.Status, WeightedLaptopEstimateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedLaptopEstimateMovesToQueued()
    {
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("code-152", "title-599");
        try
        {
            item.MoveTo(WeightedLaptopEstimateStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedLaptopEstimateStatus.Queued, "status");
    }

    public static void WeightedLaptopEstimateRejectsMovesFromFailed()
    {
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("code-152", "title-599");
        item.Status = WeightedLaptopEstimateStatus.Failed;
        Check.Fails(() => item.MoveTo(WeightedLaptopEstimateStatus.New), "move");
    }

    public static void RunAll()
    {
        WeightedLaptopEstimateStartsInNew();
        WeightedLaptopEstimateMovesToQueued();
        WeightedLaptopEstimateRejectsMovesFromFailed();
    }
}
