namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientRecognitionBatchChecks
{
    public static void LenientRecognitionBatchStartsInCreated()
    {
        var item = LenientRecognitionBatch.NewLenientRecognitionBatch("code-578", "label-370");
        Check.Equal(item.Status, LenientRecognitionBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRecognitionBatchMovesToRetired()
    {
        var item = LenientRecognitionBatch.NewLenientRecognitionBatch("code-578", "label-370");
        try
        {
            item.MoveTo(LenientRecognitionBatchStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRecognitionBatchStatus.Retired, "status");
    }

    public static void LenientRecognitionBatchRejectsMovesFromRetired()
    {
        var item = LenientRecognitionBatch.NewLenientRecognitionBatch("code-578", "label-370");
        item.Status = LenientRecognitionBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientRecognitionBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientRecognitionBatchStartsInCreated();
        LenientRecognitionBatchMovesToRetired();
        LenientRecognitionBatchRejectsMovesFromRetired();
    }
}
