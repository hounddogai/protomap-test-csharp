namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SuccessionPlanClaimChecks
{
    public static void SuccessionPlanClaimStartsInCreated()
    {
        var item = SuccessionPlanClaim.NewSuccessionPlanClaim("code-552", "title-732");
        Check.Equal(item.Status, SuccessionPlanClaimStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanClaimMovesToValidated()
    {
        var item = SuccessionPlanClaim.NewSuccessionPlanClaim("code-552", "title-732");
        try
        {
            item.MoveTo(SuccessionPlanClaimStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanClaimStatus.Validated, "status");
    }

    public static void SuccessionPlanClaimRejectsMovesFromRetired()
    {
        var item = SuccessionPlanClaim.NewSuccessionPlanClaim("code-552", "title-732");
        item.Status = SuccessionPlanClaimStatus.Retired;
        Check.Fails(() => item.MoveTo(SuccessionPlanClaimStatus.Created), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanClaimStartsInCreated();
        SuccessionPlanClaimMovesToValidated();
        SuccessionPlanClaimRejectsMovesFromRetired();
    }
}
