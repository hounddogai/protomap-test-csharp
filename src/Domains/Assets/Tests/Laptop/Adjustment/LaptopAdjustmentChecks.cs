namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopAdjustmentChecks
{
    public static void LaptopAdjustmentStartsInPlanned()
    {
        var item = LaptopAdjustment.NewLaptopAdjustment("code-881", "title-835");
        Check.Equal(item.Status, LaptopAdjustmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopAdjustmentMovesToScheduled()
    {
        var item = LaptopAdjustment.NewLaptopAdjustment("code-881", "title-835");
        try
        {
            item.MoveTo(LaptopAdjustmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopAdjustmentStatus.Scheduled, "status");
    }

    public static void LaptopAdjustmentRejectsMovesFromCancelled()
    {
        var item = LaptopAdjustment.NewLaptopAdjustment("code-881", "title-835");
        item.Status = LaptopAdjustmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LaptopAdjustmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LaptopAdjustmentStartsInPlanned();
        LaptopAdjustmentMovesToScheduled();
        LaptopAdjustmentRejectsMovesFromCancelled();
    }
}
