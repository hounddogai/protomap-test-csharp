namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class PrimaryEngagementNoticeChecks
{
    public static void PrimaryEngagementNoticeStartsInNew()
    {
        var item = PrimaryEngagementNotice.NewPrimaryEngagementNotice("code-642", "label-919");
        Check.Equal(item.Status, PrimaryEngagementNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryEngagementNoticeMovesToFailed()
    {
        var item = PrimaryEngagementNotice.NewPrimaryEngagementNotice("code-642", "label-919");
        try
        {
            item.MoveTo(PrimaryEngagementNoticeStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryEngagementNoticeStatus.Failed, "status");
    }

    public static void PrimaryEngagementNoticeRejectsMovesFromFailed()
    {
        var item = PrimaryEngagementNotice.NewPrimaryEngagementNotice("code-642", "label-919");
        item.Status = PrimaryEngagementNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(PrimaryEngagementNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        PrimaryEngagementNoticeStartsInNew();
        PrimaryEngagementNoticeMovesToFailed();
        PrimaryEngagementNoticeRejectsMovesFromFailed();
    }
}
