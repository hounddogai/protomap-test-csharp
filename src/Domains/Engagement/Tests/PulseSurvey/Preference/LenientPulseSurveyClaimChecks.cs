namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientPulseSurveyClaimChecks
{
    public static void LenientPulseSurveyClaimStartsInOpen()
    {
        var item = LenientPulseSurveyClaim.NewLenientPulseSurveyClaim("code-727", "title-428");
        Check.Equal(item.Status, LenientPulseSurveyClaimStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPulseSurveyClaimMovesToInReview()
    {
        var item = LenientPulseSurveyClaim.NewLenientPulseSurveyClaim("code-727", "title-428");
        try
        {
            item.MoveTo(LenientPulseSurveyClaimStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPulseSurveyClaimStatus.InReview, "status");
    }

    public static void LenientPulseSurveyClaimRejectsMovesFromArchived()
    {
        var item = LenientPulseSurveyClaim.NewLenientPulseSurveyClaim("code-727", "title-428");
        item.Status = LenientPulseSurveyClaimStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientPulseSurveyClaimStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientPulseSurveyClaimStartsInOpen();
        LenientPulseSurveyClaimMovesToInReview();
        LenientPulseSurveyClaimRejectsMovesFromArchived();
    }
}
