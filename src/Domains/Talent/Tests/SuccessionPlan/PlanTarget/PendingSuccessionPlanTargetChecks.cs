namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class PendingSuccessionPlanTargetChecks
{
    public static void PendingSuccessionPlanTargetStartsInRequested()
    {
        var item = PendingSuccessionPlanTarget.NewPendingSuccessionPlanTarget("code-335", "label-582");
        Check.Equal(item.Status, PendingSuccessionPlanTargetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingSuccessionPlanTargetMovesToExpired()
    {
        var item = PendingSuccessionPlanTarget.NewPendingSuccessionPlanTarget("code-335", "label-582");
        try
        {
            item.MoveTo(PendingSuccessionPlanTargetStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingSuccessionPlanTargetStatus.Expired, "status");
    }

    public static void PendingSuccessionPlanTargetRejectsMovesFromExpired()
    {
        var item = PendingSuccessionPlanTarget.NewPendingSuccessionPlanTarget("code-335", "label-582");
        item.Status = PendingSuccessionPlanTargetStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingSuccessionPlanTargetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingSuccessionPlanTargetStartsInRequested();
        PendingSuccessionPlanTargetMovesToExpired();
        PendingSuccessionPlanTargetRejectsMovesFromExpired();
    }
}
