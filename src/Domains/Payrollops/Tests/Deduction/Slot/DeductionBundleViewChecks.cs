namespace Acme.Workforce.Payrollops.Tests.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeductionBundleViewChecks
{
    public static void DeductionBundleViewShowsTheStatusInCapitals()
    {
        var item = DeductionBundle.NewDeductionBundle("code-799", "name-899");
        var view = DeductionBundleView.ToDeductionBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionBundleView.ToDeductionBundleViews(new List<DeductionBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionBundleViewShowsTheStatusInCapitals();
    }
}
