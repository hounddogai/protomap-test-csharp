namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopVarianceChecks
{
    public static void LaptopVarianceStartsInCreated()
    {
        var item = LaptopVariance.NewLaptopVariance("code-925", "title-582");
        Check.Equal(item.Status, LaptopVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopVarianceMovesToValidated()
    {
        var item = LaptopVariance.NewLaptopVariance("code-925", "title-582");
        try
        {
            item.MoveTo(LaptopVarianceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopVarianceStatus.Validated, "status");
    }

    public static void LaptopVarianceRejectsMovesFromRetired()
    {
        var item = LaptopVariance.NewLaptopVariance("code-925", "title-582");
        item.Status = LaptopVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(LaptopVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        LaptopVarianceStartsInCreated();
        LaptopVarianceMovesToValidated();
        LaptopVarianceRejectsMovesFromRetired();
    }
}
