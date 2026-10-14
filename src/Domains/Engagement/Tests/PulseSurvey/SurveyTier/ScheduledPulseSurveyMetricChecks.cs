namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyMetricChecks
{
    public static void ScheduledPulseSurveyMetricStartsInIdle()
    {
        var item = ScheduledPulseSurveyMetric.NewScheduledPulseSurveyMetric("code-649", "title-197");
        Check.Equal(item.Status, ScheduledPulseSurveyMetricStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPulseSurveyMetricMovesToReady()
    {
        var item = ScheduledPulseSurveyMetric.NewScheduledPulseSurveyMetric("code-649", "title-197");
        try
        {
            item.MoveTo(ScheduledPulseSurveyMetricStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPulseSurveyMetricStatus.Ready, "status");
    }

    public static void ScheduledPulseSurveyMetricRejectsMovesFromStopped()
    {
        var item = ScheduledPulseSurveyMetric.NewScheduledPulseSurveyMetric("code-649", "title-197");
        item.Status = ScheduledPulseSurveyMetricStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledPulseSurveyMetricStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledPulseSurveyMetricStartsInIdle();
        ScheduledPulseSurveyMetricMovesToReady();
        ScheduledPulseSurveyMetricRejectsMovesFromStopped();
    }
}
