namespace Acme.Workforce.Orgdesign.Tests.Headcount.Proposal;

using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AdaptiveHeadcountAuditViewChecks
{
    public static void AdaptiveHeadcountAuditViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("code-192", "label-847");
        var view = AdaptiveHeadcountAuditView.ToAdaptiveHeadcountAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveHeadcountAuditView.ToAdaptiveHeadcountAuditViews(new List<AdaptiveHeadcountAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountAuditViewShowsTheStatusInCapitals();
    }
}
