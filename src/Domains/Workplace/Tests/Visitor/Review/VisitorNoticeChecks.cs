namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VisitorNoticeChecks
{
    public static void VisitorNoticeStartsInRequested()
    {
        var item = VisitorNotice.NewVisitorNotice("code-770", "name-864");
        Check.Equal(item.Status, VisitorNoticeStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorNoticeMovesToGranted()
    {
        var item = VisitorNotice.NewVisitorNotice("code-770", "name-864");
        try
        {
            item.MoveTo(VisitorNoticeStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorNoticeStatus.Granted, "status");
    }

    public static void VisitorNoticeRejectsMovesFromExpired()
    {
        var item = VisitorNotice.NewVisitorNotice("code-770", "name-864");
        item.Status = VisitorNoticeStatus.Expired;
        Check.Fails(() => item.MoveTo(VisitorNoticeStatus.Requested), "move");
    }

    public static void RunAll()
    {
        VisitorNoticeStartsInRequested();
        VisitorNoticeMovesToGranted();
        VisitorNoticeRejectsMovesFromExpired();
    }
}
