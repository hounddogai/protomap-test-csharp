namespace Acme.Workforce.Compliance.Tests.TrainingDue.Schedule;

using Acme.Workforce.Compliance.TrainingDue.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TrainingDueMetricChecks
{
    public static void TrainingDueMetricStartsInDraft()
    {
        var item = TrainingDueMetric.NewTrainingDueMetric("code-376", "title-902");
        Check.Equal(item.Status, TrainingDueMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueMetricMovesToClosed()
    {
        var item = TrainingDueMetric.NewTrainingDueMetric("code-376", "title-902");
        try
        {
            item.MoveTo(TrainingDueMetricStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueMetricStatus.Closed, "status");
    }

    public static void TrainingDueMetricRejectsMovesFromClosed()
    {
        var item = TrainingDueMetric.NewTrainingDueMetric("code-376", "title-902");
        item.Status = TrainingDueMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(TrainingDueMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TrainingDueMetricStartsInDraft();
        TrainingDueMetricMovesToClosed();
        TrainingDueMetricRejectsMovesFromClosed();
    }
}
