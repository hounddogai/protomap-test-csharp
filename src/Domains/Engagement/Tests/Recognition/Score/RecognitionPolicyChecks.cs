namespace Acme.Workforce.Engagement.Tests.Recognition.Score;

using Acme.Workforce.Engagement.Recognition.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RecognitionPolicyChecks
{
    public static void RecognitionPolicyStartsInCreated()
    {
        var item = RecognitionPolicy.NewRecognitionPolicy("code-70", "title-271");
        Check.Equal(item.Status, RecognitionPolicyStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionPolicyMovesToRetired()
    {
        var item = RecognitionPolicy.NewRecognitionPolicy("code-70", "title-271");
        try
        {
            item.MoveTo(RecognitionPolicyStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionPolicyStatus.Retired, "status");
    }

    public static void RecognitionPolicyRejectsMovesFromRetired()
    {
        var item = RecognitionPolicy.NewRecognitionPolicy("code-70", "title-271");
        item.Status = RecognitionPolicyStatus.Retired;
        Check.Fails(() => item.MoveTo(RecognitionPolicyStatus.Created), "move");
    }

    public static void RunAll()
    {
        RecognitionPolicyStartsInCreated();
        RecognitionPolicyMovesToRetired();
        RecognitionPolicyRejectsMovesFromRetired();
    }
}
