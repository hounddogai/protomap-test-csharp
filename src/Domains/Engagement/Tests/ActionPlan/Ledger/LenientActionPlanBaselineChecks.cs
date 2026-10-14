namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LenientActionPlanBaselineChecks
{
    public static void LenientActionPlanBaselineStartsInIdle()
    {
        var item = LenientActionPlanBaseline.NewLenientActionPlanBaseline("code-137", "label-818");
        Check.Equal(item.Status, LenientActionPlanBaselineStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientActionPlanBaselineMovesToWarming()
    {
        var item = LenientActionPlanBaseline.NewLenientActionPlanBaseline("code-137", "label-818");
        try
        {
            item.MoveTo(LenientActionPlanBaselineStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientActionPlanBaselineStatus.Warming, "status");
    }

    public static void LenientActionPlanBaselineRejectsMovesFromStopped()
    {
        var item = LenientActionPlanBaseline.NewLenientActionPlanBaseline("code-137", "label-818");
        item.Status = LenientActionPlanBaselineStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientActionPlanBaselineStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientActionPlanBaselineStartsInIdle();
        LenientActionPlanBaselineMovesToWarming();
        LenientActionPlanBaselineRejectsMovesFromStopped();
    }
}
