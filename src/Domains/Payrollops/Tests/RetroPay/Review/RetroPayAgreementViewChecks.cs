namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RetroPayAgreementViewChecks
{
    public static void RetroPayAgreementViewShowsTheStatusInCapitals()
    {
        var item = RetroPayAgreement.NewRetroPayAgreement("code-470", "name-498");
        var view = RetroPayAgreementView.ToRetroPayAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayAgreementView.ToRetroPayAgreementViews(new List<RetroPayAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayAgreementViewShowsTheStatusInCapitals();
    }
}
