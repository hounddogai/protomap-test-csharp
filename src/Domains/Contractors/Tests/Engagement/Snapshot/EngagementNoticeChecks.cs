namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementNoticeChecks
{
    public static void EngagementNoticeStartsInCreated()
    {
        var item = EngagementNotice.NewEngagementNotice("code-647", "label-959");
        Check.Equal(item.Status, EngagementNoticeStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementNoticeMovesToValidated()
    {
        var item = EngagementNotice.NewEngagementNotice("code-647", "label-959");
        try
        {
            item.MoveTo(EngagementNoticeStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementNoticeStatus.Validated, "status");
    }

    public static void EngagementNoticeRejectsMovesFromRetired()
    {
        var item = EngagementNotice.NewEngagementNotice("code-647", "label-959");
        item.Status = EngagementNoticeStatus.Retired;
        Check.Fails(() => item.MoveTo(EngagementNoticeStatus.Created), "move");
    }

    public static void RunAll()
    {
        EngagementNoticeStartsInCreated();
        EngagementNoticeMovesToValidated();
        EngagementNoticeRejectsMovesFromRetired();
    }
}
