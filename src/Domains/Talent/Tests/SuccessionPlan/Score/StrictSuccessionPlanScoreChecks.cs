namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanScoreChecks
{
    public static void StrictSuccessionPlanScoreStartsInProposed()
    {
        var item = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore("code-121", "name-150");
        Check.Equal(item.Status, StrictSuccessionPlanScoreStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictSuccessionPlanScoreMovesToApproved()
    {
        var item = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore("code-121", "name-150");
        try
        {
            item.MoveTo(StrictSuccessionPlanScoreStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictSuccessionPlanScoreStatus.Approved, "status");
    }

    public static void StrictSuccessionPlanScoreRejectsMovesFromWithdrawn()
    {
        var item = StrictSuccessionPlanScore.NewStrictSuccessionPlanScore("code-121", "name-150");
        item.Status = StrictSuccessionPlanScoreStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictSuccessionPlanScoreStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictSuccessionPlanScoreStartsInProposed();
        StrictSuccessionPlanScoreMovesToApproved();
        StrictSuccessionPlanScoreRejectsMovesFromWithdrawn();
    }
}
