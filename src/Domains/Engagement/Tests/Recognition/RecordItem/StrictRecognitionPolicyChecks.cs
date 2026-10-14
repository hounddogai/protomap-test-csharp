namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictRecognitionPolicyChecks
{
    public static void StrictRecognitionPolicyStartsInNew()
    {
        var item = StrictRecognitionPolicy.NewStrictRecognitionPolicy("code-36", "name-84");
        Check.Equal(item.Status, StrictRecognitionPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictRecognitionPolicyMovesToDone()
    {
        var item = StrictRecognitionPolicy.NewStrictRecognitionPolicy("code-36", "name-84");
        try
        {
            item.MoveTo(StrictRecognitionPolicyStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictRecognitionPolicyStatus.Done, "status");
    }

    public static void StrictRecognitionPolicyRejectsMovesFromFailed()
    {
        var item = StrictRecognitionPolicy.NewStrictRecognitionPolicy("code-36", "name-84");
        item.Status = StrictRecognitionPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictRecognitionPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictRecognitionPolicyStartsInNew();
        StrictRecognitionPolicyMovesToDone();
        StrictRecognitionPolicyRejectsMovesFromFailed();
    }
}
