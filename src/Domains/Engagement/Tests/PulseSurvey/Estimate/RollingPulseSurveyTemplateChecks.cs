namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RollingPulseSurveyTemplateChecks
{
    public static void RollingPulseSurveyTemplateStartsInPlanned()
    {
        var item = RollingPulseSurveyTemplate.NewRollingPulseSurveyTemplate("code-253", "title-119");
        Check.Equal(item.Status, RollingPulseSurveyTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPulseSurveyTemplateMovesToInProgress()
    {
        var item = RollingPulseSurveyTemplate.NewRollingPulseSurveyTemplate("code-253", "title-119");
        try
        {
            item.MoveTo(RollingPulseSurveyTemplateStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPulseSurveyTemplateStatus.InProgress, "status");
    }

    public static void RollingPulseSurveyTemplateRejectsMovesFromCancelled()
    {
        var item = RollingPulseSurveyTemplate.NewRollingPulseSurveyTemplate("code-253", "title-119");
        item.Status = RollingPulseSurveyTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingPulseSurveyTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingPulseSurveyTemplateStartsInPlanned();
        RollingPulseSurveyTemplateMovesToInProgress();
        RollingPulseSurveyTemplateRejectsMovesFromCancelled();
    }
}
