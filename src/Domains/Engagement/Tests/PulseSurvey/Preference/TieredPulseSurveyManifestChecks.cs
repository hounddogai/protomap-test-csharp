namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TieredPulseSurveyManifestChecks
{
    public static void TieredPulseSurveyManifestStartsInOpen()
    {
        var item = TieredPulseSurveyManifest.NewTieredPulseSurveyManifest("code-231", "title-680");
        Check.Equal(item.Status, TieredPulseSurveyManifestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPulseSurveyManifestMovesToResolved()
    {
        var item = TieredPulseSurveyManifest.NewTieredPulseSurveyManifest("code-231", "title-680");
        try
        {
            item.MoveTo(TieredPulseSurveyManifestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPulseSurveyManifestStatus.Resolved, "status");
    }

    public static void TieredPulseSurveyManifestRejectsMovesFromArchived()
    {
        var item = TieredPulseSurveyManifest.NewTieredPulseSurveyManifest("code-231", "title-680");
        item.Status = TieredPulseSurveyManifestStatus.Archived;
        Check.Fails(() => item.MoveTo(TieredPulseSurveyManifestStatus.Open), "move");
    }

    public static void RunAll()
    {
        TieredPulseSurveyManifestStartsInOpen();
        TieredPulseSurveyManifestMovesToResolved();
        TieredPulseSurveyManifestRejectsMovesFromArchived();
    }
}
