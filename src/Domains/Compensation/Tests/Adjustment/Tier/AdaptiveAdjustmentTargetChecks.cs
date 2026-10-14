namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AdaptiveAdjustmentTargetChecks
{
    public static void AdaptiveAdjustmentTargetStartsInCreated()
    {
        var item = AdaptiveAdjustmentTarget.NewAdaptiveAdjustmentTarget("code-180", "label-521");
        Check.Equal(item.Status, AdaptiveAdjustmentTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAdjustmentTargetMovesToValidated()
    {
        var item = AdaptiveAdjustmentTarget.NewAdaptiveAdjustmentTarget("code-180", "label-521");
        try
        {
            item.MoveTo(AdaptiveAdjustmentTargetStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAdjustmentTargetStatus.Validated, "status");
    }

    public static void AdaptiveAdjustmentTargetRejectsMovesFromRetired()
    {
        var item = AdaptiveAdjustmentTarget.NewAdaptiveAdjustmentTarget("code-180", "label-521");
        item.Status = AdaptiveAdjustmentTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveAdjustmentTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveAdjustmentTargetStartsInCreated();
        AdaptiveAdjustmentTargetMovesToValidated();
        AdaptiveAdjustmentTargetRejectsMovesFromRetired();
    }
}
