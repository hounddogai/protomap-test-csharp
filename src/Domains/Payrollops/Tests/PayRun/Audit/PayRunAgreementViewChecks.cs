namespace Acme.Workforce.Payrollops.Tests.PayRun.Audit;

using Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PayRunAgreementViewChecks
{
    public static void PayRunAgreementViewShowsTheStatusInCapitals()
    {
        var item = PayRunAgreement.NewPayRunAgreement("code-816", "name-763");
        var view = PayRunAgreementView.ToPayRunAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunAgreementView.ToPayRunAgreementViews(new List<PayRunAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunAgreementViewShowsTheStatusInCapitals();
    }
}
