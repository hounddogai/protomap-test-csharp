namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SeasonalTrainingDueMetricChecks
{
    public static void SeasonalTrainingDueMetricStartsInDraft()
    {
        var item = SeasonalTrainingDueMetric.NewSeasonalTrainingDueMetric("code-564", "title-56");
        Check.Equal(item.Status, SeasonalTrainingDueMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalTrainingDueMetricMovesToActive()
    {
        var item = SeasonalTrainingDueMetric.NewSeasonalTrainingDueMetric("code-564", "title-56");
        try
        {
            item.MoveTo(SeasonalTrainingDueMetricStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalTrainingDueMetricStatus.Active, "status");
    }

    public static void SeasonalTrainingDueMetricRejectsMovesFromClosed()
    {
        var item = SeasonalTrainingDueMetric.NewSeasonalTrainingDueMetric("code-564", "title-56");
        item.Status = SeasonalTrainingDueMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalTrainingDueMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalTrainingDueMetricStartsInDraft();
        SeasonalTrainingDueMetricMovesToActive();
        SeasonalTrainingDueMetricRejectsMovesFromClosed();
    }
}
