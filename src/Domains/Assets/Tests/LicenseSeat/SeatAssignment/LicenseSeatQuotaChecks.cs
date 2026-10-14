namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class LicenseSeatQuotaChecks
{
    public static void LicenseSeatQuotaStartsInDraft()
    {
        var item = LicenseSeatQuota.NewLicenseSeatQuota("code-259", "name-776");
        Check.Equal(item.Status, LicenseSeatQuotaStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatQuotaMovesToSuspended()
    {
        var item = LicenseSeatQuota.NewLicenseSeatQuota("code-259", "name-776");
        try
        {
            item.MoveTo(LicenseSeatQuotaStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatQuotaStatus.Suspended, "status");
    }

    public static void LicenseSeatQuotaRejectsMovesFromClosed()
    {
        var item = LicenseSeatQuota.NewLicenseSeatQuota("code-259", "name-776");
        item.Status = LicenseSeatQuotaStatus.Closed;
        Check.Fails(() => item.MoveTo(LicenseSeatQuotaStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LicenseSeatQuotaStartsInDraft();
        LicenseSeatQuotaMovesToSuspended();
        LicenseSeatQuotaRejectsMovesFromClosed();
    }
}
