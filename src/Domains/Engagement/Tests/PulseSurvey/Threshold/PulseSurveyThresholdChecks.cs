namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyThresholdChecks
{
    public static void PulseSurveyThresholdStartsInOpen()
    {
        var item = PulseSurveyThreshold.NewPulseSurveyThreshold("code-926", "label-997");
        Check.Equal(item.Status, PulseSurveyThresholdStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyThresholdMovesToInReview()
    {
        var item = PulseSurveyThreshold.NewPulseSurveyThreshold("code-926", "label-997");
        try
        {
            item.MoveTo(PulseSurveyThresholdStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyThresholdStatus.InReview, "status");
    }

    public static void PulseSurveyThresholdRejectsMovesFromArchived()
    {
        var item = PulseSurveyThreshold.NewPulseSurveyThreshold("code-926", "label-997");
        item.Status = PulseSurveyThresholdStatus.Archived;
        Check.Fails(() => item.MoveTo(PulseSurveyThresholdStatus.Open), "move");
    }

    public static void RunAll()
    {
        PulseSurveyThresholdStartsInOpen();
        PulseSurveyThresholdMovesToInReview();
        PulseSurveyThresholdRejectsMovesFromArchived();
    }
}
