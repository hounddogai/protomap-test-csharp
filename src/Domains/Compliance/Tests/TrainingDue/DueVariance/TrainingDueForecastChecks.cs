namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueForecastChecks
{
    public static void TrainingDueForecastStartsInPlanned()
    {
        var item = TrainingDueForecast.NewTrainingDueForecast("code-897", "label-566");
        Check.Equal(item.Status, TrainingDueForecastStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueForecastMovesToInProgress()
    {
        var item = TrainingDueForecast.NewTrainingDueForecast("code-897", "label-566");
        try
        {
            item.MoveTo(TrainingDueForecastStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueForecastStatus.InProgress, "status");
    }

    public static void TrainingDueForecastRejectsMovesFromCancelled()
    {
        var item = TrainingDueForecast.NewTrainingDueForecast("code-897", "label-566");
        item.Status = TrainingDueForecastStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TrainingDueForecastStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TrainingDueForecastStartsInPlanned();
        TrainingDueForecastMovesToInProgress();
        TrainingDueForecastRejectsMovesFromCancelled();
    }
}
