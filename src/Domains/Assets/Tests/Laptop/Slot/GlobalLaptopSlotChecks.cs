namespace Acme.Workforce.Assets.Tests.Laptop.Slot;

using Acme.Workforce.Assets.Laptop.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class GlobalLaptopSlotChecks
{
    public static void GlobalLaptopSlotStartsInCreated()
    {
        var item = GlobalLaptopSlot.NewGlobalLaptopSlot("code-624", "name-563");
        Check.Equal(item.Status, GlobalLaptopSlotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalLaptopSlotMovesToPublished()
    {
        var item = GlobalLaptopSlot.NewGlobalLaptopSlot("code-624", "name-563");
        try
        {
            item.MoveTo(GlobalLaptopSlotStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalLaptopSlotStatus.Published, "status");
    }

    public static void GlobalLaptopSlotRejectsMovesFromRetired()
    {
        var item = GlobalLaptopSlot.NewGlobalLaptopSlot("code-624", "name-563");
        item.Status = GlobalLaptopSlotStatus.Retired;
        Check.Fails(() => item.MoveTo(GlobalLaptopSlotStatus.Created), "move");
    }

    public static void RunAll()
    {
        GlobalLaptopSlotStartsInCreated();
        GlobalLaptopSlotMovesToPublished();
        GlobalLaptopSlotRejectsMovesFromRetired();
    }
}
