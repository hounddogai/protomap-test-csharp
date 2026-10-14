namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanClaim;

using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Checks of plan claim code, run by the project's runner.</summary>
public static class LenientSuccessionPlanClaimChecks
{
    public static void LenientSuccessionPlanClaimStartsInProposed()
    {
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("code-597", "label-734");
        Check.Equal(item.Status, LenientSuccessionPlanClaimStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSuccessionPlanClaimMovesToRejected()
    {
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("code-597", "label-734");
        try
        {
            item.MoveTo(LenientSuccessionPlanClaimStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSuccessionPlanClaimStatus.Rejected, "status");
    }

    public static void LenientSuccessionPlanClaimRejectsMovesFromWithdrawn()
    {
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("code-597", "label-734");
        item.Status = LenientSuccessionPlanClaimStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientSuccessionPlanClaimStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanClaimStartsInProposed();
        LenientSuccessionPlanClaimMovesToRejected();
        LenientSuccessionPlanClaimRejectsMovesFromWithdrawn();
    }
}
