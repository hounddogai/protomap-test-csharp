namespace Acme.Workforce.Engagement.Tests.PulseSurvey.OverrideItem;

using Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictPulseSurveyOverrideChecks
{
    public static void StrictPulseSurveyOverrideStartsInProposed()
    {
        var item = StrictPulseSurveyOverride.NewStrictPulseSurveyOverride("code-805", "label-768");
        Check.Equal(item.Status, StrictPulseSurveyOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPulseSurveyOverrideMovesToApproved()
    {
        var item = StrictPulseSurveyOverride.NewStrictPulseSurveyOverride("code-805", "label-768");
        try
        {
            item.MoveTo(StrictPulseSurveyOverrideStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPulseSurveyOverrideStatus.Approved, "status");
    }

    public static void StrictPulseSurveyOverrideRejectsMovesFromWithdrawn()
    {
        var item = StrictPulseSurveyOverride.NewStrictPulseSurveyOverride("code-805", "label-768");
        item.Status = StrictPulseSurveyOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictPulseSurveyOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictPulseSurveyOverrideStartsInProposed();
        StrictPulseSurveyOverrideMovesToApproved();
        StrictPulseSurveyOverrideRejectsMovesFromWithdrawn();
    }
}
