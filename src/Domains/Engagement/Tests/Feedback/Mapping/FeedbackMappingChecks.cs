namespace Acme.Workforce.Engagement.Tests.Feedback.Mapping;

using Acme.Workforce.Engagement.Feedback.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FeedbackMappingChecks
{
    public static void FeedbackMappingStartsInNew()
    {
        var item = FeedbackMapping.NewFeedbackMapping("code-542", "title-91");
        Check.Equal(item.Status, FeedbackMappingStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackMappingMovesToDone()
    {
        var item = FeedbackMapping.NewFeedbackMapping("code-542", "title-91");
        try
        {
            item.MoveTo(FeedbackMappingStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackMappingStatus.Done, "status");
    }

    public static void FeedbackMappingRejectsMovesFromFailed()
    {
        var item = FeedbackMapping.NewFeedbackMapping("code-542", "title-91");
        item.Status = FeedbackMappingStatus.Failed;
        Check.Fails(() => item.MoveTo(FeedbackMappingStatus.New), "move");
    }

    public static void RunAll()
    {
        FeedbackMappingStartsInNew();
        FeedbackMappingMovesToDone();
        FeedbackMappingRejectsMovesFromFailed();
    }
}
