namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class FallbackLicenseSeatAuditChecks
{
    public static void FallbackLicenseSeatAuditStartsInDraft()
    {
        var item = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit("code-313", "name-298");
        Check.Equal(item.Status, FallbackLicenseSeatAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackLicenseSeatAuditMovesToActive()
    {
        var item = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit("code-313", "name-298");
        try
        {
            item.MoveTo(FallbackLicenseSeatAuditStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackLicenseSeatAuditStatus.Active, "status");
    }

    public static void FallbackLicenseSeatAuditRejectsMovesFromClosed()
    {
        var item = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit("code-313", "name-298");
        item.Status = FallbackLicenseSeatAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackLicenseSeatAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatAuditStartsInDraft();
        FallbackLicenseSeatAuditMovesToActive();
        FallbackLicenseSeatAuditRejectsMovesFromClosed();
    }
}
