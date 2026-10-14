namespace Acme.Workforce.Assets.Tests.LicenseSeat.Cycle;

using Acme.Workforce.Assets.LicenseSeat.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LenientLicenseSeatAuditViewChecks
{
    public static void LenientLicenseSeatAuditViewShowsTheStatusInCapitals()
    {
        var item = LenientLicenseSeatAudit.NewLenientLicenseSeatAudit("code-116", "title-916");
        var view = LenientLicenseSeatAuditView.ToLenientLicenseSeatAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientLicenseSeatAuditView.ToLenientLicenseSeatAuditViews(new List<LenientLicenseSeatAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientLicenseSeatAuditViewShowsTheStatusInCapitals();
    }
}
