namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveTrainingDuePlanChecks
{
    public static void AdaptiveTrainingDuePlanStartsInRequested()
    {
        var item = AdaptiveTrainingDuePlan.NewAdaptiveTrainingDuePlan("code-987", "label-708");
        Check.Equal(item.Status, AdaptiveTrainingDuePlanStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTrainingDuePlanMovesToExpired()
    {
        var item = AdaptiveTrainingDuePlan.NewAdaptiveTrainingDuePlan("code-987", "label-708");
        try
        {
            item.MoveTo(AdaptiveTrainingDuePlanStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTrainingDuePlanStatus.Expired, "status");
    }

    public static void AdaptiveTrainingDuePlanRejectsMovesFromExpired()
    {
        var item = AdaptiveTrainingDuePlan.NewAdaptiveTrainingDuePlan("code-987", "label-708");
        item.Status = AdaptiveTrainingDuePlanStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveTrainingDuePlanStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDuePlanStartsInRequested();
        AdaptiveTrainingDuePlanMovesToExpired();
        AdaptiveTrainingDuePlanRejectsMovesFromExpired();
    }
}
