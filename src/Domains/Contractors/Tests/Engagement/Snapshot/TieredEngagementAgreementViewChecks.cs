namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class TieredEngagementAgreementViewChecks
{
    public static void TieredEngagementAgreementViewShowsTheStatusInCapitals()
    {
        var item = TieredEngagementAgreement.NewTieredEngagementAgreement("code-279", "title-646");
        var view = TieredEngagementAgreementView.ToTieredEngagementAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredEngagementAgreementView.ToTieredEngagementAgreementViews(new List<TieredEngagementAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredEngagementAgreementViewShowsTheStatusInCapitals();
    }
}
