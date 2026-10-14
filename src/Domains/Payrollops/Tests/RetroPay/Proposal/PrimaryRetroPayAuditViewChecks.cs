namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class PrimaryRetroPayAuditViewChecks
{
    public static void PrimaryRetroPayAuditViewShowsTheStatusInCapitals()
    {
        var item = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit("code-326", "label-744");
        var view = PrimaryRetroPayAuditView.ToPrimaryRetroPayAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryRetroPayAuditView.ToPrimaryRetroPayAuditViews(new List<PrimaryRetroPayAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryRetroPayAuditViewShowsTheStatusInCapitals();
    }
}
