namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalPayslipBaselineViewChecks
{
    public static void SeasonalPayslipBaselineViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("code-581", "label-999");
        var view = SeasonalPayslipBaselineView.ToSeasonalPayslipBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPayslipBaselineView.ToSeasonalPayslipBaselineViews(new List<SeasonalPayslipBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPayslipBaselineViewShowsTheStatusInCapitals();
    }
}
