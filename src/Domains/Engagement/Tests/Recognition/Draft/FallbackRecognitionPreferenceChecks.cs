namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackRecognitionPreferenceChecks
{
    public static void FallbackRecognitionPreferenceStartsInNew()
    {
        var item = FallbackRecognitionPreference.NewFallbackRecognitionPreference("code-654", "name-317");
        Check.Equal(item.Status, FallbackRecognitionPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackRecognitionPreferenceMovesToDone()
    {
        var item = FallbackRecognitionPreference.NewFallbackRecognitionPreference("code-654", "name-317");
        try
        {
            item.MoveTo(FallbackRecognitionPreferenceStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackRecognitionPreferenceStatus.Done, "status");
    }

    public static void FallbackRecognitionPreferenceRejectsMovesFromFailed()
    {
        var item = FallbackRecognitionPreference.NewFallbackRecognitionPreference("code-654", "name-317");
        item.Status = FallbackRecognitionPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackRecognitionPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackRecognitionPreferenceStartsInNew();
        FallbackRecognitionPreferenceMovesToDone();
        FallbackRecognitionPreferenceRejectsMovesFromFailed();
    }
}
