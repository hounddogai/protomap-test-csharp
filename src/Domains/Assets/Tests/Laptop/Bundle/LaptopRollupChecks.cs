namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopRollupChecks
{
    public static void LaptopRollupStartsInCreated()
    {
        var item = LaptopRollup.NewLaptopRollup("code-340", "title-102");
        Check.Equal(item.Status, LaptopRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopRollupMovesToValidated()
    {
        var item = LaptopRollup.NewLaptopRollup("code-340", "title-102");
        try
        {
            item.MoveTo(LaptopRollupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopRollupStatus.Validated, "status");
    }

    public static void LaptopRollupRejectsMovesFromRetired()
    {
        var item = LaptopRollup.NewLaptopRollup("code-340", "title-102");
        item.Status = LaptopRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(LaptopRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        LaptopRollupStartsInCreated();
        LaptopRollupMovesToValidated();
        LaptopRollupRejectsMovesFromRetired();
    }
}
