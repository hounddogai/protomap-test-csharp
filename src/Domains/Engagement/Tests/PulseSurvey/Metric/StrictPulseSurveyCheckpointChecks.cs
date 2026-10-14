namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class StrictPulseSurveyCheckpointChecks
{
    public static void StrictPulseSurveyCheckpointStartsInIdle()
    {
        var item = StrictPulseSurveyCheckpoint.NewStrictPulseSurveyCheckpoint("code-341", "label-921");
        Check.Equal(item.Status, StrictPulseSurveyCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPulseSurveyCheckpointMovesToDraining()
    {
        var item = StrictPulseSurveyCheckpoint.NewStrictPulseSurveyCheckpoint("code-341", "label-921");
        try
        {
            item.MoveTo(StrictPulseSurveyCheckpointStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPulseSurveyCheckpointStatus.Draining, "status");
    }

    public static void StrictPulseSurveyCheckpointRejectsMovesFromStopped()
    {
        var item = StrictPulseSurveyCheckpoint.NewStrictPulseSurveyCheckpoint("code-341", "label-921");
        item.Status = StrictPulseSurveyCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictPulseSurveyCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictPulseSurveyCheckpointStartsInIdle();
        StrictPulseSurveyCheckpointMovesToDraining();
        StrictPulseSurveyCheckpointRejectsMovesFromStopped();
    }
}
