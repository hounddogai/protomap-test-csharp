namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class SeasonalRetroPayClaimChecks
{
    public static void SeasonalRetroPayClaimStartsInCreated()
    {
        var item = SeasonalRetroPayClaim.NewSeasonalRetroPayClaim("code-530", "name-8");
        Check.Equal(item.Status, SeasonalRetroPayClaimStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalRetroPayClaimMovesToRetired()
    {
        var item = SeasonalRetroPayClaim.NewSeasonalRetroPayClaim("code-530", "name-8");
        try
        {
            item.MoveTo(SeasonalRetroPayClaimStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalRetroPayClaimStatus.Retired, "status");
    }

    public static void SeasonalRetroPayClaimRejectsMovesFromRetired()
    {
        var item = SeasonalRetroPayClaim.NewSeasonalRetroPayClaim("code-530", "name-8");
        item.Status = SeasonalRetroPayClaimStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalRetroPayClaimStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalRetroPayClaimStartsInCreated();
        SeasonalRetroPayClaimMovesToRetired();
        SeasonalRetroPayClaimRejectsMovesFromRetired();
    }
}
