namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PulseSurveyMetricChecks
{
    public static void PulseSurveyMetricStartsInOpen()
    {
        var item = PulseSurveyMetric.NewPulseSurveyMetric("code-663", "label-649");
        Check.Equal(item.Status, PulseSurveyMetricStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyMetricMovesToReopened()
    {
        var item = PulseSurveyMetric.NewPulseSurveyMetric("code-663", "label-649");
        try
        {
            item.MoveTo(PulseSurveyMetricStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyMetricStatus.Reopened, "status");
    }

    public static void PulseSurveyMetricRejectsMovesFromArchived()
    {
        var item = PulseSurveyMetric.NewPulseSurveyMetric("code-663", "label-649");
        item.Status = PulseSurveyMetricStatus.Archived;
        Check.Fails(() => item.MoveTo(PulseSurveyMetricStatus.Open), "move");
    }

    public static void RunAll()
    {
        PulseSurveyMetricStartsInOpen();
        PulseSurveyMetricMovesToReopened();
        PulseSurveyMetricRejectsMovesFromArchived();
    }
}
