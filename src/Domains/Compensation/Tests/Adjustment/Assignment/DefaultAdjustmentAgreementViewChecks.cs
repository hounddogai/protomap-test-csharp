namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DefaultAdjustmentAgreementViewChecks
{
    public static void DefaultAdjustmentAgreementViewShowsTheStatusInCapitals()
    {
        var item = DefaultAdjustmentAgreement.NewDefaultAdjustmentAgreement("code-9", "title-487");
        var view = DefaultAdjustmentAgreementView.ToDefaultAdjustmentAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAdjustmentAgreementView.ToDefaultAdjustmentAgreementViews(new List<DefaultAdjustmentAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAdjustmentAgreementViewShowsTheStatusInCapitals();
    }
}
