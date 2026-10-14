namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

/// <summary>Checks of adjustment agreement code, run by the project's runner.</summary>
public static class PendingAdjustmentAgreementViewChecks
{
    public static void PendingAdjustmentAgreementViewShowsTheStatusInCapitals()
    {
        var item = PendingAdjustmentAgreement.NewPendingAdjustmentAgreement("code-376", "title-670");
        var view = PendingAdjustmentAgreementView.ToPendingAdjustmentAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingAdjustmentAgreementView.ToPendingAdjustmentAgreementViews(new List<PendingAdjustmentAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingAdjustmentAgreementViewShowsTheStatusInCapitals();
    }
}
