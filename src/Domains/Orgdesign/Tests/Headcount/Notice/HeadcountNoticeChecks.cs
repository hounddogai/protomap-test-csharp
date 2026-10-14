namespace Acme.Workforce.Orgdesign.Tests.Headcount.Notice;

using Acme.Workforce.Orgdesign.Headcount.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class HeadcountNoticeChecks
{
    public static void HeadcountNoticeStartsInProposed()
    {
        var item = HeadcountNotice.NewHeadcountNotice("code-706", "label-358");
        Check.Equal(item.Status, HeadcountNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountNoticeMovesToRejected()
    {
        var item = HeadcountNotice.NewHeadcountNotice("code-706", "label-358");
        try
        {
            item.MoveTo(HeadcountNoticeStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountNoticeStatus.Rejected, "status");
    }

    public static void HeadcountNoticeRejectsMovesFromWithdrawn()
    {
        var item = HeadcountNotice.NewHeadcountNotice("code-706", "label-358");
        item.Status = HeadcountNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(HeadcountNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        HeadcountNoticeStartsInProposed();
        HeadcountNoticeMovesToRejected();
        HeadcountNoticeRejectsMovesFromWithdrawn();
    }
}
