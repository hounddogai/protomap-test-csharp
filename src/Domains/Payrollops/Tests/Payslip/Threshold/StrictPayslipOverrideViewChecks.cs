namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StrictPayslipOverrideViewChecks
{
    public static void StrictPayslipOverrideViewShowsTheStatusInCapitals()
    {
        var item = StrictPayslipOverride.NewStrictPayslipOverride("code-870", "title-956");
        var view = StrictPayslipOverrideView.ToStrictPayslipOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPayslipOverrideView.ToStrictPayslipOverrideViews(new List<StrictPayslipOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPayslipOverrideViewShowsTheStatusInCapitals();
    }
}
