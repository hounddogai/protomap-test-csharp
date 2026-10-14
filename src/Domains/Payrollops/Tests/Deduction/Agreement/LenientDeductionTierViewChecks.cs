namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientDeductionTierViewChecks
{
    public static void LenientDeductionTierViewShowsTheStatusInCapitals()
    {
        var item = LenientDeductionTier.NewLenientDeductionTier("code-380", "name-549");
        var view = LenientDeductionTierView.ToLenientDeductionTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientDeductionTierView.ToLenientDeductionTierViews(new List<LenientDeductionTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientDeductionTierViewShowsTheStatusInCapitals();
    }
}
