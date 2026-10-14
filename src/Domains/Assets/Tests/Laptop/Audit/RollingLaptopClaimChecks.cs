namespace Acme.Workforce.Assets.Tests.Laptop.Audit;

using Acme.Workforce.Assets.Laptop.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingLaptopClaimChecks
{
    public static void RollingLaptopClaimStartsInOpen()
    {
        var item = RollingLaptopClaim.NewRollingLaptopClaim("code-455", "name-772");
        Check.Equal(item.Status, RollingLaptopClaimStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingLaptopClaimMovesToInReview()
    {
        var item = RollingLaptopClaim.NewRollingLaptopClaim("code-455", "name-772");
        try
        {
            item.MoveTo(RollingLaptopClaimStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingLaptopClaimStatus.InReview, "status");
    }

    public static void RollingLaptopClaimRejectsMovesFromArchived()
    {
        var item = RollingLaptopClaim.NewRollingLaptopClaim("code-455", "name-772");
        item.Status = RollingLaptopClaimStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingLaptopClaimStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingLaptopClaimStartsInOpen();
        RollingLaptopClaimMovesToInReview();
        RollingLaptopClaimRejectsMovesFromArchived();
    }
}
