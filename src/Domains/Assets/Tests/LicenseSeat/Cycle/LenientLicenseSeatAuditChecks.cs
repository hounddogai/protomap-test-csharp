namespace Acme.Workforce.Assets.Tests.LicenseSeat.Cycle;

using Acme.Workforce.Assets.LicenseSeat.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LenientLicenseSeatAuditChecks
{
    public static void LenientLicenseSeatAuditStartsInCreated()
    {
        var item = LenientLicenseSeatAudit.NewLenientLicenseSeatAudit("code-116", "title-916");
        Check.Equal(item.Status, LenientLicenseSeatAuditStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLicenseSeatAuditMovesToPublished()
    {
        var item = LenientLicenseSeatAudit.NewLenientLicenseSeatAudit("code-116", "title-916");
        try
        {
            item.MoveTo(LenientLicenseSeatAuditStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLicenseSeatAuditStatus.Published, "status");
    }

    public static void LenientLicenseSeatAuditRejectsMovesFromRetired()
    {
        var item = LenientLicenseSeatAudit.NewLenientLicenseSeatAudit("code-116", "title-916");
        item.Status = LenientLicenseSeatAuditStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientLicenseSeatAuditStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientLicenseSeatAuditStartsInCreated();
        LenientLicenseSeatAuditMovesToPublished();
        LenientLicenseSeatAuditRejectsMovesFromRetired();
    }
}
