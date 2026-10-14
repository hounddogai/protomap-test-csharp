namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatAuditChecks
{
    public static void LicenseSeatAuditStartsInRequested()
    {
        var item = LicenseSeatAudit.NewLicenseSeatAudit("code-686", "title-16");
        Check.Equal(item.Status, LicenseSeatAuditStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatAuditMovesToExpired()
    {
        var item = LicenseSeatAudit.NewLicenseSeatAudit("code-686", "title-16");
        try
        {
            item.MoveTo(LicenseSeatAuditStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatAuditStatus.Expired, "status");
    }

    public static void LicenseSeatAuditRejectsMovesFromExpired()
    {
        var item = LicenseSeatAudit.NewLicenseSeatAudit("code-686", "title-16");
        item.Status = LicenseSeatAuditStatus.Expired;
        Check.Fails(() => item.MoveTo(LicenseSeatAuditStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LicenseSeatAuditStartsInRequested();
        LicenseSeatAuditMovesToExpired();
        LicenseSeatAuditRejectsMovesFromExpired();
    }
}
