namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionPreferenceChecks
{
    public static void RecognitionPreferenceStartsInProposed()
    {
        var item = RecognitionPreference.NewRecognitionPreference("code-961", "label-235");
        Check.Equal(item.Status, RecognitionPreferenceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionPreferenceMovesToWithdrawn()
    {
        var item = RecognitionPreference.NewRecognitionPreference("code-961", "label-235");
        try
        {
            item.MoveTo(RecognitionPreferenceStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionPreferenceStatus.Withdrawn, "status");
    }

    public static void RecognitionPreferenceRejectsMovesFromWithdrawn()
    {
        var item = RecognitionPreference.NewRecognitionPreference("code-961", "label-235");
        item.Status = RecognitionPreferenceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RecognitionPreferenceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RecognitionPreferenceStartsInProposed();
        RecognitionPreferenceMovesToWithdrawn();
        RecognitionPreferenceRejectsMovesFromWithdrawn();
    }
}
