namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAgreement;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Checks of seat agreement code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatNoticeChecks
{
    public static void AdaptiveLicenseSeatNoticeStartsInRequested()
    {
        var item = AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice("code-343", "title-551");
        Check.Equal(item.Status, AdaptiveLicenseSeatNoticeStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveLicenseSeatNoticeMovesToExpired()
    {
        var item = AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice("code-343", "title-551");
        try
        {
            item.MoveTo(AdaptiveLicenseSeatNoticeStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveLicenseSeatNoticeStatus.Expired, "status");
    }

    public static void AdaptiveLicenseSeatNoticeRejectsMovesFromExpired()
    {
        var item = AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice("code-343", "title-551");
        item.Status = AdaptiveLicenseSeatNoticeStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveLicenseSeatNoticeStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveLicenseSeatNoticeStartsInRequested();
        AdaptiveLicenseSeatNoticeMovesToExpired();
        AdaptiveLicenseSeatNoticeRejectsMovesFromExpired();
    }
}
