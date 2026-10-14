namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledPayslipTemplateViewChecks
{
    public static void ScheduledPayslipTemplateViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPayslipTemplate.NewScheduledPayslipTemplate("code-288", "label-328");
        var view = ScheduledPayslipTemplateView.ToScheduledPayslipTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPayslipTemplateView.ToScheduledPayslipTemplateViews(new List<ScheduledPayslipTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPayslipTemplateViewShowsTheStatusInCapitals();
    }
}
