namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopBaselineChecks
{
    public static void LaptopBaselineStartsInPlanned()
    {
        var item = LaptopBaseline.NewLaptopBaseline("code-593", "label-612");
        Check.Equal(item.Status, LaptopBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopBaselineMovesToScheduled()
    {
        var item = LaptopBaseline.NewLaptopBaseline("code-593", "label-612");
        try
        {
            item.MoveTo(LaptopBaselineStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopBaselineStatus.Scheduled, "status");
    }

    public static void LaptopBaselineRejectsMovesFromCancelled()
    {
        var item = LaptopBaseline.NewLaptopBaseline("code-593", "label-612");
        item.Status = LaptopBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LaptopBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LaptopBaselineStartsInPlanned();
        LaptopBaselineMovesToScheduled();
        LaptopBaselineRejectsMovesFromCancelled();
    }
}
