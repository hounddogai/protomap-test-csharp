namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LenientPulseSurveyManifestChecks
{
    public static void LenientPulseSurveyManifestStartsInNew()
    {
        var item = LenientPulseSurveyManifest.NewLenientPulseSurveyManifest("code-604", "label-301");
        Check.Equal(item.Status, LenientPulseSurveyManifestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPulseSurveyManifestMovesToDone()
    {
        var item = LenientPulseSurveyManifest.NewLenientPulseSurveyManifest("code-604", "label-301");
        try
        {
            item.MoveTo(LenientPulseSurveyManifestStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPulseSurveyManifestStatus.Done, "status");
    }

    public static void LenientPulseSurveyManifestRejectsMovesFromFailed()
    {
        var item = LenientPulseSurveyManifest.NewLenientPulseSurveyManifest("code-604", "label-301");
        item.Status = LenientPulseSurveyManifestStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientPulseSurveyManifestStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientPulseSurveyManifestStartsInNew();
        LenientPulseSurveyManifestMovesToDone();
        LenientPulseSurveyManifestRejectsMovesFromFailed();
    }
}
