namespace Acme.Workforce.Payrollops.Tests.Payslip.Mapping;

using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultPayslipThresholdViewChecks
{
    public static void DefaultPayslipThresholdViewShowsTheStatusInCapitals()
    {
        var item = DefaultPayslipThreshold.NewDefaultPayslipThreshold("code-36", "title-893");
        var view = DefaultPayslipThresholdView.ToDefaultPayslipThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPayslipThresholdView.ToDefaultPayslipThresholdViews(new List<DefaultPayslipThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPayslipThresholdViewShowsTheStatusInCapitals();
    }
}
