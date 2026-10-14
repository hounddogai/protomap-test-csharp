namespace Acme.Workforce.Engagement.Tests.Recognition.Preference;

using Acme.Workforce.Engagement.Recognition.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientRecognitionPreferenceChecks
{
    public static void LenientRecognitionPreferenceStartsInOpen()
    {
        var item = LenientRecognitionPreference.NewLenientRecognitionPreference("code-889", "title-161");
        Check.Equal(item.Status, LenientRecognitionPreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRecognitionPreferenceMovesToArchived()
    {
        var item = LenientRecognitionPreference.NewLenientRecognitionPreference("code-889", "title-161");
        try
        {
            item.MoveTo(LenientRecognitionPreferenceStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRecognitionPreferenceStatus.Archived, "status");
    }

    public static void LenientRecognitionPreferenceRejectsMovesFromArchived()
    {
        var item = LenientRecognitionPreference.NewLenientRecognitionPreference("code-889", "title-161");
        item.Status = LenientRecognitionPreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientRecognitionPreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientRecognitionPreferenceStartsInOpen();
        LenientRecognitionPreferenceMovesToArchived();
        LenientRecognitionPreferenceRejectsMovesFromArchived();
    }
}
