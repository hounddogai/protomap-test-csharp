namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SuccessionPlanScoreChecks
{
    public static void SuccessionPlanScoreStartsInProposed()
    {
        var item = SuccessionPlanScore.NewSuccessionPlanScore("code-492", "title-253");
        Check.Equal(item.Status, SuccessionPlanScoreStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanScoreMovesToApproved()
    {
        var item = SuccessionPlanScore.NewSuccessionPlanScore("code-492", "title-253");
        try
        {
            item.MoveTo(SuccessionPlanScoreStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanScoreStatus.Approved, "status");
    }

    public static void SuccessionPlanScoreRejectsMovesFromWithdrawn()
    {
        var item = SuccessionPlanScore.NewSuccessionPlanScore("code-492", "title-253");
        item.Status = SuccessionPlanScoreStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SuccessionPlanScoreStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanScoreStartsInProposed();
        SuccessionPlanScoreMovesToApproved();
        SuccessionPlanScoreRejectsMovesFromWithdrawn();
    }
}
