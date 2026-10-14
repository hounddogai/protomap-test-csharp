namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class WeightedLaptopThresholdChecks
{
    public static void WeightedLaptopThresholdStartsInRequested()
    {
        var item = WeightedLaptopThreshold.NewWeightedLaptopThreshold("code-614", "title-675");
        Check.Equal(item.Status, WeightedLaptopThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedLaptopThresholdMovesToRevoked()
    {
        var item = WeightedLaptopThreshold.NewWeightedLaptopThreshold("code-614", "title-675");
        try
        {
            item.MoveTo(WeightedLaptopThresholdStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedLaptopThresholdStatus.Revoked, "status");
    }

    public static void WeightedLaptopThresholdRejectsMovesFromExpired()
    {
        var item = WeightedLaptopThreshold.NewWeightedLaptopThreshold("code-614", "title-675");
        item.Status = WeightedLaptopThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedLaptopThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedLaptopThresholdStartsInRequested();
        WeightedLaptopThresholdMovesToRevoked();
        WeightedLaptopThresholdRejectsMovesFromExpired();
    }
}
