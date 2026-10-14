namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationAuditViewChecks
{
    public static void NominationAuditViewShowsTheStatusInCapitals()
    {
        var item = NominationAudit.NewNominationAudit("code-156", "name-536");
        var view = NominationAuditView.ToNominationAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationAuditView.ToNominationAuditViews(new List<NominationAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationAuditViewShowsTheStatusInCapitals();
    }
}
