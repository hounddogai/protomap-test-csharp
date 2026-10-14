namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientDeductionAgreementViewChecks
{
    public static void LenientDeductionAgreementViewShowsTheStatusInCapitals()
    {
        var item = LenientDeductionAgreement.NewLenientDeductionAgreement("code-859", "title-29");
        var view = LenientDeductionAgreementView.ToLenientDeductionAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientDeductionAgreementView.ToLenientDeductionAgreementViews(new List<LenientDeductionAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientDeductionAgreementViewShowsTheStatusInCapitals();
    }
}
