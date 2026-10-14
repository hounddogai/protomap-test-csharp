namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyReviewChecks
{
    public static void PulseSurveyReviewStartsInDraft()
    {
        var item = PulseSurveyReview.NewPulseSurveyReview("code-703", "label-588");
        Check.Equal(item.Status, PulseSurveyReviewStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyReviewMovesToActive()
    {
        var item = PulseSurveyReview.NewPulseSurveyReview("code-703", "label-588");
        try
        {
            item.MoveTo(PulseSurveyReviewStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyReviewStatus.Active, "status");
    }

    public static void PulseSurveyReviewRejectsMovesFromClosed()
    {
        var item = PulseSurveyReview.NewPulseSurveyReview("code-703", "label-588");
        item.Status = PulseSurveyReviewStatus.Closed;
        Check.Fails(() => item.MoveTo(PulseSurveyReviewStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PulseSurveyReviewStartsInDraft();
        PulseSurveyReviewMovesToActive();
        PulseSurveyReviewRejectsMovesFromClosed();
    }
}
