namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LaptopOverrideChecks
{
    public static void LaptopOverrideStartsInRequested()
    {
        var item = LaptopOverride.NewLaptopOverride("code-510", "title-261");
        Check.Equal(item.Status, LaptopOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopOverrideMovesToGranted()
    {
        var item = LaptopOverride.NewLaptopOverride("code-510", "title-261");
        try
        {
            item.MoveTo(LaptopOverrideStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopOverrideStatus.Granted, "status");
    }

    public static void LaptopOverrideRejectsMovesFromExpired()
    {
        var item = LaptopOverride.NewLaptopOverride("code-510", "title-261");
        item.Status = LaptopOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(LaptopOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LaptopOverrideStartsInRequested();
        LaptopOverrideMovesToGranted();
        LaptopOverrideRejectsMovesFromExpired();
    }
}
