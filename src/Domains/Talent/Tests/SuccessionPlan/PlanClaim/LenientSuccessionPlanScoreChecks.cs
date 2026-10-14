namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanClaim;

using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Checks of plan claim code, run by the project's runner.</summary>
public static class LenientSuccessionPlanScoreChecks
{
    public static void LenientSuccessionPlanScoreStartsInRequested()
    {
        var item = LenientSuccessionPlanScore.NewLenientSuccessionPlanScore("code-332", "label-470");
        Check.Equal(item.Status, LenientSuccessionPlanScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSuccessionPlanScoreMovesToGranted()
    {
        var item = LenientSuccessionPlanScore.NewLenientSuccessionPlanScore("code-332", "label-470");
        try
        {
            item.MoveTo(LenientSuccessionPlanScoreStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSuccessionPlanScoreStatus.Granted, "status");
    }

    public static void LenientSuccessionPlanScoreRejectsMovesFromExpired()
    {
        var item = LenientSuccessionPlanScore.NewLenientSuccessionPlanScore("code-332", "label-470");
        item.Status = LenientSuccessionPlanScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientSuccessionPlanScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanScoreStartsInRequested();
        LenientSuccessionPlanScoreMovesToGranted();
        LenientSuccessionPlanScoreRejectsMovesFromExpired();
    }
}
