namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyTemplateChecks
{
    public static void PulseSurveyTemplateStartsInNew()
    {
        var item = PulseSurveyTemplate.NewPulseSurveyTemplate("code-358", "label-299");
        Check.Equal(item.Status, PulseSurveyTemplateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyTemplateMovesToFailed()
    {
        var item = PulseSurveyTemplate.NewPulseSurveyTemplate("code-358", "label-299");
        try
        {
            item.MoveTo(PulseSurveyTemplateStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyTemplateStatus.Failed, "status");
    }

    public static void PulseSurveyTemplateRejectsMovesFromFailed()
    {
        var item = PulseSurveyTemplate.NewPulseSurveyTemplate("code-358", "label-299");
        item.Status = PulseSurveyTemplateStatus.Failed;
        Check.Fails(() => item.MoveTo(PulseSurveyTemplateStatus.New), "move");
    }

    public static void RunAll()
    {
        PulseSurveyTemplateStartsInNew();
        PulseSurveyTemplateMovesToFailed();
        PulseSurveyTemplateRejectsMovesFromFailed();
    }
}
