namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Target;

using Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientSuccessionPlanTargetChecks
{
    public static void LenientSuccessionPlanTargetStartsInPlanned()
    {
        var item = LenientSuccessionPlanTarget.NewLenientSuccessionPlanTarget("code-131", "title-438");
        Check.Equal(item.Status, LenientSuccessionPlanTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSuccessionPlanTargetMovesToScheduled()
    {
        var item = LenientSuccessionPlanTarget.NewLenientSuccessionPlanTarget("code-131", "title-438");
        try
        {
            item.MoveTo(LenientSuccessionPlanTargetStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSuccessionPlanTargetStatus.Scheduled, "status");
    }

    public static void LenientSuccessionPlanTargetRejectsMovesFromCancelled()
    {
        var item = LenientSuccessionPlanTarget.NewLenientSuccessionPlanTarget("code-131", "title-438");
        item.Status = LenientSuccessionPlanTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientSuccessionPlanTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanTargetStartsInPlanned();
        LenientSuccessionPlanTargetMovesToScheduled();
        LenientSuccessionPlanTargetRejectsMovesFromCancelled();
    }
}
