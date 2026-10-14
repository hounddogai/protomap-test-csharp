namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LaptopEstimateChecks
{
    public static void LaptopEstimateStartsInDraft()
    {
        var item = LaptopEstimate.NewLaptopEstimate("code-337", "name-574");
        Check.Equal(item.Status, LaptopEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopEstimateMovesToActive()
    {
        var item = LaptopEstimate.NewLaptopEstimate("code-337", "name-574");
        try
        {
            item.MoveTo(LaptopEstimateStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopEstimateStatus.Active, "status");
    }

    public static void LaptopEstimateRejectsMovesFromClosed()
    {
        var item = LaptopEstimate.NewLaptopEstimate("code-337", "name-574");
        item.Status = LaptopEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(LaptopEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LaptopEstimateStartsInDraft();
        LaptopEstimateMovesToActive();
        LaptopEstimateRejectsMovesFromClosed();
    }
}
