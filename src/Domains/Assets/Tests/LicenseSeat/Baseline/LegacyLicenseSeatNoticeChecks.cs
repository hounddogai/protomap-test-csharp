namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LegacyLicenseSeatNoticeChecks
{
    public static void LegacyLicenseSeatNoticeStartsInProposed()
    {
        var item = LegacyLicenseSeatNotice.NewLegacyLicenseSeatNotice("code-890", "label-837");
        Check.Equal(item.Status, LegacyLicenseSeatNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyLicenseSeatNoticeMovesToApproved()
    {
        var item = LegacyLicenseSeatNotice.NewLegacyLicenseSeatNotice("code-890", "label-837");
        try
        {
            item.MoveTo(LegacyLicenseSeatNoticeStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyLicenseSeatNoticeStatus.Approved, "status");
    }

    public static void LegacyLicenseSeatNoticeRejectsMovesFromWithdrawn()
    {
        var item = LegacyLicenseSeatNotice.NewLegacyLicenseSeatNotice("code-890", "label-837");
        item.Status = LegacyLicenseSeatNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyLicenseSeatNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyLicenseSeatNoticeStartsInProposed();
        LegacyLicenseSeatNoticeMovesToApproved();
        LegacyLicenseSeatNoticeRejectsMovesFromWithdrawn();
    }
}
