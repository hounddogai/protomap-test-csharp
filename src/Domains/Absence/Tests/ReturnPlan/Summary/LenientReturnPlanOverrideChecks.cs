namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LenientReturnPlanOverrideChecks
{
    public static void LenientReturnPlanOverrideStartsInPlanned()
    {
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("code-554", "name-821");
        Check.Equal(item.Status, LenientReturnPlanOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientReturnPlanOverrideMovesToScheduled()
    {
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("code-554", "name-821");
        try
        {
            item.MoveTo(LenientReturnPlanOverrideStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientReturnPlanOverrideStatus.Scheduled, "status");
    }

    public static void LenientReturnPlanOverrideRejectsMovesFromCancelled()
    {
        var item = LenientReturnPlanOverride.NewLenientReturnPlanOverride("code-554", "name-821");
        item.Status = LenientReturnPlanOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientReturnPlanOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientReturnPlanOverrideStartsInPlanned();
        LenientReturnPlanOverrideMovesToScheduled();
        LenientReturnPlanOverrideRejectsMovesFromCancelled();
    }
}
