namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class LaptopClaimChecks
{
    public static void LaptopClaimStartsInNew()
    {
        var item = LaptopClaim.NewLaptopClaim("code-98", "name-931");
        Check.Equal(item.Status, LaptopClaimStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopClaimMovesToDone()
    {
        var item = LaptopClaim.NewLaptopClaim("code-98", "name-931");
        try
        {
            item.MoveTo(LaptopClaimStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopClaimStatus.Done, "status");
    }

    public static void LaptopClaimRejectsMovesFromFailed()
    {
        var item = LaptopClaim.NewLaptopClaim("code-98", "name-931");
        item.Status = LaptopClaimStatus.Failed;
        Check.Fails(() => item.MoveTo(LaptopClaimStatus.New), "move");
    }

    public static void RunAll()
    {
        LaptopClaimStartsInNew();
        LaptopClaimMovesToDone();
        LaptopClaimRejectsMovesFromFailed();
    }
}
