namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveRecognitionBatchChecks
{
    public static void AdaptiveRecognitionBatchStartsInCreated()
    {
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("code-203", "label-860");
        Check.Equal(item.Status, AdaptiveRecognitionBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveRecognitionBatchMovesToPublished()
    {
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("code-203", "label-860");
        try
        {
            item.MoveTo(AdaptiveRecognitionBatchStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveRecognitionBatchStatus.Published, "status");
    }

    public static void AdaptiveRecognitionBatchRejectsMovesFromRetired()
    {
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("code-203", "label-860");
        item.Status = AdaptiveRecognitionBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveRecognitionBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveRecognitionBatchStartsInCreated();
        AdaptiveRecognitionBatchMovesToPublished();
        AdaptiveRecognitionBatchRejectsMovesFromRetired();
    }
}
