namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class FallbackLicenseSeatAuditViewChecks
{
    public static void FallbackLicenseSeatAuditViewShowsTheStatusInCapitals()
    {
        var item = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit("code-313", "name-298");
        var view = FallbackLicenseSeatAuditView.ToFallbackLicenseSeatAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackLicenseSeatAuditView.ToFallbackLicenseSeatAuditViews(new List<FallbackLicenseSeatAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatAuditViewShowsTheStatusInCapitals();
    }
}
