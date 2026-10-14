namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyEstimateChecks
{
    public static void ScheduledPulseSurveyEstimateStartsInOpen()
    {
        var item = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate("code-170", "title-555");
        Check.Equal(item.Status, ScheduledPulseSurveyEstimateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPulseSurveyEstimateMovesToReopened()
    {
        var item = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate("code-170", "title-555");
        try
        {
            item.MoveTo(ScheduledPulseSurveyEstimateStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPulseSurveyEstimateStatus.Reopened, "status");
    }

    public static void ScheduledPulseSurveyEstimateRejectsMovesFromArchived()
    {
        var item = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate("code-170", "title-555");
        item.Status = ScheduledPulseSurveyEstimateStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledPulseSurveyEstimateStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledPulseSurveyEstimateStartsInOpen();
        ScheduledPulseSurveyEstimateMovesToReopened();
        ScheduledPulseSurveyEstimateRejectsMovesFromArchived();
    }
}
