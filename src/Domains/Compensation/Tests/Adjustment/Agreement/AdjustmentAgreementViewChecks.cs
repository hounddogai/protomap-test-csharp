namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentAgreementViewChecks
{
    public static void AdjustmentAgreementViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentAgreement.NewAdjustmentAgreement("code-290", "name-80");
        var view = AdjustmentAgreementView.ToAdjustmentAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentAgreementView.ToAdjustmentAgreementViews(new List<AdjustmentAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentAgreementViewShowsTheStatusInCapitals();
    }
}
