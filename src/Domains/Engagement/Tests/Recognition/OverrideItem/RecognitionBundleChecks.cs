namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionBundleChecks
{
    public static void RecognitionBundleStartsInOpen()
    {
        var item = RecognitionBundle.NewRecognitionBundle("code-813", "name-920");
        Check.Equal(item.Status, RecognitionBundleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionBundleMovesToReopened()
    {
        var item = RecognitionBundle.NewRecognitionBundle("code-813", "name-920");
        try
        {
            item.MoveTo(RecognitionBundleStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionBundleStatus.Reopened, "status");
    }

    public static void RecognitionBundleRejectsMovesFromArchived()
    {
        var item = RecognitionBundle.NewRecognitionBundle("code-813", "name-920");
        item.Status = RecognitionBundleStatus.Archived;
        Check.Fails(() => item.MoveTo(RecognitionBundleStatus.Open), "move");
    }

    public static void RunAll()
    {
        RecognitionBundleStartsInOpen();
        RecognitionBundleMovesToReopened();
        RecognitionBundleRejectsMovesFromArchived();
    }
}
