namespace Acme.Workforce.Assets.Tests.LicenseSeat.Milestone;

using Acme.Workforce.Assets.LicenseSeat.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LicenseSeatNoticeChecks
{
    public static void LicenseSeatNoticeStartsInNew()
    {
        var item = LicenseSeatNotice.NewLicenseSeatNotice("code-941", "title-40");
        Check.Equal(item.Status, LicenseSeatNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatNoticeMovesToDone()
    {
        var item = LicenseSeatNotice.NewLicenseSeatNotice("code-941", "title-40");
        try
        {
            item.MoveTo(LicenseSeatNoticeStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatNoticeStatus.Done, "status");
    }

    public static void LicenseSeatNoticeRejectsMovesFromFailed()
    {
        var item = LicenseSeatNotice.NewLicenseSeatNotice("code-941", "title-40");
        item.Status = LicenseSeatNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(LicenseSeatNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        LicenseSeatNoticeStartsInNew();
        LicenseSeatNoticeMovesToDone();
        LicenseSeatNoticeRejectsMovesFromFailed();
    }
}
