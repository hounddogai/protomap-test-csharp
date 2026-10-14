namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanTargetChecks
{
    public static void SuccessionPlanTargetStartsInIdle()
    {
        var item = SuccessionPlanTarget.NewSuccessionPlanTarget("code-377", "name-459");
        Check.Equal(item.Status, SuccessionPlanTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanTargetMovesToReady()
    {
        var item = SuccessionPlanTarget.NewSuccessionPlanTarget("code-377", "name-459");
        try
        {
            item.MoveTo(SuccessionPlanTargetStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanTargetStatus.Ready, "status");
    }

    public static void SuccessionPlanTargetRejectsMovesFromStopped()
    {
        var item = SuccessionPlanTarget.NewSuccessionPlanTarget("code-377", "name-459");
        item.Status = SuccessionPlanTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(SuccessionPlanTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanTargetStartsInIdle();
        SuccessionPlanTargetMovesToReady();
        SuccessionPlanTargetRejectsMovesFromStopped();
    }
}
