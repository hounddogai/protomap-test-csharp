namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueMetricChecks
{
    public static void AdaptiveTrainingDueMetricStartsInCreated()
    {
        var item = AdaptiveTrainingDueMetric.NewAdaptiveTrainingDueMetric("code-322", "label-365");
        Check.Equal(item.Status, AdaptiveTrainingDueMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTrainingDueMetricMovesToValidated()
    {
        var item = AdaptiveTrainingDueMetric.NewAdaptiveTrainingDueMetric("code-322", "label-365");
        try
        {
            item.MoveTo(AdaptiveTrainingDueMetricStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTrainingDueMetricStatus.Validated, "status");
    }

    public static void AdaptiveTrainingDueMetricRejectsMovesFromRetired()
    {
        var item = AdaptiveTrainingDueMetric.NewAdaptiveTrainingDueMetric("code-322", "label-365");
        item.Status = AdaptiveTrainingDueMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveTrainingDueMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDueMetricStartsInCreated();
        AdaptiveTrainingDueMetricMovesToValidated();
        AdaptiveTrainingDueMetricRejectsMovesFromRetired();
    }
}
