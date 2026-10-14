namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementAgreementViewChecks
{
    public static void EngagementAgreementViewShowsTheStatusInCapitals()
    {
        var item = EngagementAgreement.NewEngagementAgreement("code-580", "label-439");
        var view = EngagementAgreementView.ToEngagementAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementAgreementView.ToEngagementAgreementViews(new List<EngagementAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementAgreementViewShowsTheStatusInCapitals();
    }
}
