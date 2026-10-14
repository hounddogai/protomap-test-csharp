namespace Acme.Workforce.Assets.Tests.Laptop.Baseline;

using Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LaptopSlotChecks
{
    public static void LaptopSlotStartsInNew()
    {
        var item = LaptopSlot.NewLaptopSlot("code-230", "title-118");
        Check.Equal(item.Status, LaptopSlotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopSlotMovesToQueued()
    {
        var item = LaptopSlot.NewLaptopSlot("code-230", "title-118");
        try
        {
            item.MoveTo(LaptopSlotStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopSlotStatus.Queued, "status");
    }

    public static void LaptopSlotRejectsMovesFromFailed()
    {
        var item = LaptopSlot.NewLaptopSlot("code-230", "title-118");
        item.Status = LaptopSlotStatus.Failed;
        Check.Fails(() => item.MoveTo(LaptopSlotStatus.New), "move");
    }

    public static void RunAll()
    {
        LaptopSlotStartsInNew();
        LaptopSlotMovesToQueued();
        LaptopSlotRejectsMovesFromFailed();
    }
}
