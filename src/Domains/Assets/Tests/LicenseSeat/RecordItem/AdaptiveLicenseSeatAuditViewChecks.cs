namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatAuditViewChecks
{
    public static void AdaptiveLicenseSeatAuditViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("code-627", "label-563");
        var view = AdaptiveLicenseSeatAuditView.ToAdaptiveLicenseSeatAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveLicenseSeatAuditView.ToAdaptiveLicenseSeatAuditViews(new List<AdaptiveLicenseSeatAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveLicenseSeatAuditViewShowsTheStatusInCapitals();
    }
}
