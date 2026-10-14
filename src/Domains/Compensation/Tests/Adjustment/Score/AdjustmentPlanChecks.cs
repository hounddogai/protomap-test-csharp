namespace Acme.Workforce.Compensation.Tests.Adjustment.Score;

using Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdjustmentPlanChecks
{
    public static void AdjustmentPlanStartsInRequested()
    {
        var item = AdjustmentPlan.NewAdjustmentPlan("code-836", "label-183");
        Check.Equal(item.Status, AdjustmentPlanStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentPlanMovesToGranted()
    {
        var item = AdjustmentPlan.NewAdjustmentPlan("code-836", "label-183");
        try
        {
            item.MoveTo(AdjustmentPlanStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentPlanStatus.Granted, "status");
    }

    public static void AdjustmentPlanRejectsMovesFromExpired()
    {
        var item = AdjustmentPlan.NewAdjustmentPlan("code-836", "label-183");
        item.Status = AdjustmentPlanStatus.Expired;
        Check.Fails(() => item.MoveTo(AdjustmentPlanStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdjustmentPlanStartsInRequested();
        AdjustmentPlanMovesToGranted();
        AdjustmentPlanRejectsMovesFromExpired();
    }
}
