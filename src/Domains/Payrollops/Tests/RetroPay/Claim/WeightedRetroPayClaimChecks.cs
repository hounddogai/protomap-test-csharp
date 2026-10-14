namespace Acme.Workforce.Payrollops.Tests.RetroPay.Claim;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class WeightedRetroPayClaimChecks
{
    public static void WeightedRetroPayClaimStartsInCreated()
    {
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("code-796", "title-693");
        Check.Equal(item.Status, WeightedRetroPayClaimStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedRetroPayClaimMovesToPublished()
    {
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("code-796", "title-693");
        try
        {
            item.MoveTo(WeightedRetroPayClaimStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedRetroPayClaimStatus.Published, "status");
    }

    public static void WeightedRetroPayClaimRejectsMovesFromRetired()
    {
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("code-796", "title-693");
        item.Status = WeightedRetroPayClaimStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedRetroPayClaimStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedRetroPayClaimStartsInCreated();
        WeightedRetroPayClaimMovesToPublished();
        WeightedRetroPayClaimRejectsMovesFromRetired();
    }
}
