namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LegacyRecognitionBaselineChecks
{
    public static void LegacyRecognitionBaselineStartsInDraft()
    {
        var item = LegacyRecognitionBaseline.NewLegacyRecognitionBaseline("code-462", "name-24");
        Check.Equal(item.Status, LegacyRecognitionBaselineStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyRecognitionBaselineMovesToActive()
    {
        var item = LegacyRecognitionBaseline.NewLegacyRecognitionBaseline("code-462", "name-24");
        try
        {
            item.MoveTo(LegacyRecognitionBaselineStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyRecognitionBaselineStatus.Active, "status");
    }

    public static void LegacyRecognitionBaselineRejectsMovesFromClosed()
    {
        var item = LegacyRecognitionBaseline.NewLegacyRecognitionBaseline("code-462", "name-24");
        item.Status = LegacyRecognitionBaselineStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyRecognitionBaselineStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyRecognitionBaselineStartsInDraft();
        LegacyRecognitionBaselineMovesToActive();
        LegacyRecognitionBaselineRejectsMovesFromClosed();
    }
}
