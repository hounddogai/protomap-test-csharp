namespace Acme.Workforce.Compensation.Tests.EquityGrant.Exception;

using Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class EquityGrantAgreementViewChecks
{
    public static void EquityGrantAgreementViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantAgreement.NewEquityGrantAgreement("code-471", "name-836");
        var view = EquityGrantAgreementView.ToEquityGrantAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantAgreementView.ToEquityGrantAgreementViews(new List<EquityGrantAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantAgreementViewShowsTheStatusInCapitals();
    }
}
