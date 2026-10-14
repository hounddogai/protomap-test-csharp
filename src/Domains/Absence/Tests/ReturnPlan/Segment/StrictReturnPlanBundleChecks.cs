namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictReturnPlanBundleChecks
{
    public static void StrictReturnPlanBundleStartsInPlanned()
    {
        var item = StrictReturnPlanBundle.NewStrictReturnPlanBundle("code-401", "name-834");
        Check.Equal(item.Status, StrictReturnPlanBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictReturnPlanBundleMovesToInProgress()
    {
        var item = StrictReturnPlanBundle.NewStrictReturnPlanBundle("code-401", "name-834");
        try
        {
            item.MoveTo(StrictReturnPlanBundleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictReturnPlanBundleStatus.InProgress, "status");
    }

    public static void StrictReturnPlanBundleRejectsMovesFromCancelled()
    {
        var item = StrictReturnPlanBundle.NewStrictReturnPlanBundle("code-401", "name-834");
        item.Status = StrictReturnPlanBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictReturnPlanBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictReturnPlanBundleStartsInPlanned();
        StrictReturnPlanBundleMovesToInProgress();
        StrictReturnPlanBundleRejectsMovesFromCancelled();
    }
}
