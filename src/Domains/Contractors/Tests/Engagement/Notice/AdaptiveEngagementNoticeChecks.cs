namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdaptiveEngagementNoticeChecks
{
    public static void AdaptiveEngagementNoticeStartsInNew()
    {
        var item = AdaptiveEngagementNotice.NewAdaptiveEngagementNotice("code-976", "title-996");
        Check.Equal(item.Status, AdaptiveEngagementNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveEngagementNoticeMovesToDone()
    {
        var item = AdaptiveEngagementNotice.NewAdaptiveEngagementNotice("code-976", "title-996");
        try
        {
            item.MoveTo(AdaptiveEngagementNoticeStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveEngagementNoticeStatus.Done, "status");
    }

    public static void AdaptiveEngagementNoticeRejectsMovesFromFailed()
    {
        var item = AdaptiveEngagementNotice.NewAdaptiveEngagementNotice("code-976", "title-996");
        item.Status = AdaptiveEngagementNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveEngagementNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveEngagementNoticeStartsInNew();
        AdaptiveEngagementNoticeMovesToDone();
        AdaptiveEngagementNoticeRejectsMovesFromFailed();
    }
}
