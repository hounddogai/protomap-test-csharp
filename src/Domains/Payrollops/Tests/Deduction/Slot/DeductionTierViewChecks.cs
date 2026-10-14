namespace Acme.Workforce.Payrollops.Tests.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeductionTierViewChecks
{
    public static void DeductionTierViewShowsTheStatusInCapitals()
    {
        var item = DeductionTier.NewDeductionTier("code-163", "title-291");
        var view = DeductionTierView.ToDeductionTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionTierView.ToDeductionTierViews(new List<DeductionTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionTierViewShowsTheStatusInCapitals();
    }
}
