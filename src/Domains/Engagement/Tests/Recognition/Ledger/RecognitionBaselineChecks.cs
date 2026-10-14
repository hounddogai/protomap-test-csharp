namespace Acme.Workforce.Engagement.Tests.Recognition.Ledger;

using Acme.Workforce.Engagement.Recognition.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RecognitionBaselineChecks
{
    public static void RecognitionBaselineStartsInCreated()
    {
        var item = RecognitionBaseline.NewRecognitionBaseline("code-970", "name-437");
        Check.Equal(item.Status, RecognitionBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionBaselineMovesToRetired()
    {
        var item = RecognitionBaseline.NewRecognitionBaseline("code-970", "name-437");
        try
        {
            item.MoveTo(RecognitionBaselineStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionBaselineStatus.Retired, "status");
    }

    public static void RecognitionBaselineRejectsMovesFromRetired()
    {
        var item = RecognitionBaseline.NewRecognitionBaseline("code-970", "name-437");
        item.Status = RecognitionBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(RecognitionBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        RecognitionBaselineStartsInCreated();
        RecognitionBaselineMovesToRetired();
        RecognitionBaselineRejectsMovesFromRetired();
    }
}
