namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantNoticeChecks
{
    public static void EquityGrantNoticeStartsInOpen()
    {
        var item = EquityGrantNotice.NewEquityGrantNotice("code-617", "title-243");
        Check.Equal(item.Status, EquityGrantNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantNoticeMovesToInReview()
    {
        var item = EquityGrantNotice.NewEquityGrantNotice("code-617", "title-243");
        try
        {
            item.MoveTo(EquityGrantNoticeStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantNoticeStatus.InReview, "status");
    }

    public static void EquityGrantNoticeRejectsMovesFromArchived()
    {
        var item = EquityGrantNotice.NewEquityGrantNotice("code-617", "title-243");
        item.Status = EquityGrantNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(EquityGrantNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        EquityGrantNoticeStartsInOpen();
        EquityGrantNoticeMovesToInReview();
        EquityGrantNoticeRejectsMovesFromArchived();
    }
}
