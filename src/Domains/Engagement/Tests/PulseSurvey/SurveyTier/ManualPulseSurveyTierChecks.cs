namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class ManualPulseSurveyTierChecks
{
    public static void ManualPulseSurveyTierStartsInOpen()
    {
        var item = ManualPulseSurveyTier.NewManualPulseSurveyTier("code-445", "label-314");
        Check.Equal(item.Status, ManualPulseSurveyTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPulseSurveyTierMovesToInReview()
    {
        var item = ManualPulseSurveyTier.NewManualPulseSurveyTier("code-445", "label-314");
        try
        {
            item.MoveTo(ManualPulseSurveyTierStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPulseSurveyTierStatus.InReview, "status");
    }

    public static void ManualPulseSurveyTierRejectsMovesFromArchived()
    {
        var item = ManualPulseSurveyTier.NewManualPulseSurveyTier("code-445", "label-314");
        item.Status = ManualPulseSurveyTierStatus.Archived;
        Check.Fails(() => item.MoveTo(ManualPulseSurveyTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        ManualPulseSurveyTierStartsInOpen();
        ManualPulseSurveyTierMovesToInReview();
        ManualPulseSurveyTierRejectsMovesFromArchived();
    }
}
