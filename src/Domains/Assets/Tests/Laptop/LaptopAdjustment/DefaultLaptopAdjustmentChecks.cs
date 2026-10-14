namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class DefaultLaptopAdjustmentChecks
{
    public static void DefaultLaptopAdjustmentStartsInDraft()
    {
        var item = DefaultLaptopAdjustment.NewDefaultLaptopAdjustment("code-49", "label-636");
        Check.Equal(item.Status, DefaultLaptopAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultLaptopAdjustmentMovesToPending()
    {
        var item = DefaultLaptopAdjustment.NewDefaultLaptopAdjustment("code-49", "label-636");
        try
        {
            item.MoveTo(DefaultLaptopAdjustmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultLaptopAdjustmentStatus.Pending, "status");
    }

    public static void DefaultLaptopAdjustmentRejectsMovesFromClosed()
    {
        var item = DefaultLaptopAdjustment.NewDefaultLaptopAdjustment("code-49", "label-636");
        item.Status = DefaultLaptopAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultLaptopAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultLaptopAdjustmentStartsInDraft();
        DefaultLaptopAdjustmentMovesToPending();
        DefaultLaptopAdjustmentRejectsMovesFromClosed();
    }
}
