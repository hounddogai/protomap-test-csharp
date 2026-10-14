namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class AdaptiveDeductionBaselineViewChecks
{
    public static void AdaptiveDeductionBaselineViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveDeductionBaseline.NewAdaptiveDeductionBaseline("code-969", "title-124");
        var view = AdaptiveDeductionBaselineView.ToAdaptiveDeductionBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveDeductionBaselineView.ToAdaptiveDeductionBaselineViews(new List<AdaptiveDeductionBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveDeductionBaselineViewShowsTheStatusInCapitals();
    }
}
