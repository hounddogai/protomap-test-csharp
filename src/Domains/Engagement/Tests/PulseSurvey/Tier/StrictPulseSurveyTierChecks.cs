namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictPulseSurveyTierChecks
{
    public static void StrictPulseSurveyTierStartsInDraft()
    {
        var item = StrictPulseSurveyTier.NewStrictPulseSurveyTier("code-506", "label-131");
        Check.Equal(item.Status, StrictPulseSurveyTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPulseSurveyTierMovesToActive()
    {
        var item = StrictPulseSurveyTier.NewStrictPulseSurveyTier("code-506", "label-131");
        try
        {
            item.MoveTo(StrictPulseSurveyTierStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPulseSurveyTierStatus.Active, "status");
    }

    public static void StrictPulseSurveyTierRejectsMovesFromClosed()
    {
        var item = StrictPulseSurveyTier.NewStrictPulseSurveyTier("code-506", "label-131");
        item.Status = StrictPulseSurveyTierStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictPulseSurveyTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictPulseSurveyTierStartsInDraft();
        StrictPulseSurveyTierMovesToActive();
        StrictPulseSurveyTierRejectsMovesFromClosed();
    }
}
