namespace Acme.Workforce.Absence.Tests.ReturnPlan.Threshold;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanThresholdChecks
{
    public static void AdaptiveReturnPlanThresholdStartsInDraft()
    {
        var item = AdaptiveReturnPlanThreshold.NewAdaptiveReturnPlanThreshold("code-509", "label-991");
        Check.Equal(item.Status, AdaptiveReturnPlanThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveReturnPlanThresholdMovesToActive()
    {
        var item = AdaptiveReturnPlanThreshold.NewAdaptiveReturnPlanThreshold("code-509", "label-991");
        try
        {
            item.MoveTo(AdaptiveReturnPlanThresholdStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveReturnPlanThresholdStatus.Active, "status");
    }

    public static void AdaptiveReturnPlanThresholdRejectsMovesFromClosed()
    {
        var item = AdaptiveReturnPlanThreshold.NewAdaptiveReturnPlanThreshold("code-509", "label-991");
        item.Status = AdaptiveReturnPlanThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveReturnPlanThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveReturnPlanThresholdStartsInDraft();
        AdaptiveReturnPlanThresholdMovesToActive();
        AdaptiveReturnPlanThresholdRejectsMovesFromClosed();
    }
}
