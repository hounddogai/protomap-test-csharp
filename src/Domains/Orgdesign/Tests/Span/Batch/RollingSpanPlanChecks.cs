namespace Acme.Workforce.Orgdesign.Tests.Span.Batch;

using Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RollingSpanPlanChecks
{
    public static void RollingSpanPlanStartsInPlanned()
    {
        var item = RollingSpanPlan.NewRollingSpanPlan("code-636", "name-94");
        Check.Equal(item.Status, RollingSpanPlanStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSpanPlanMovesToInProgress()
    {
        var item = RollingSpanPlan.NewRollingSpanPlan("code-636", "name-94");
        try
        {
            item.MoveTo(RollingSpanPlanStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSpanPlanStatus.InProgress, "status");
    }

    public static void RollingSpanPlanRejectsMovesFromCancelled()
    {
        var item = RollingSpanPlan.NewRollingSpanPlan("code-636", "name-94");
        item.Status = RollingSpanPlanStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingSpanPlanStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingSpanPlanStartsInPlanned();
        RollingSpanPlanMovesToInProgress();
        RollingSpanPlanRejectsMovesFromCancelled();
    }
}
