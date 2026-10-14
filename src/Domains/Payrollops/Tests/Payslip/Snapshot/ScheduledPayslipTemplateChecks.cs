namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledPayslipTemplateChecks
{
    public static void ScheduledPayslipTemplateStartsInOpen()
    {
        var item = ScheduledPayslipTemplate.NewScheduledPayslipTemplate("code-288", "label-328");
        Check.Equal(item.Status, ScheduledPayslipTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayslipTemplateMovesToInReview()
    {
        var item = ScheduledPayslipTemplate.NewScheduledPayslipTemplate("code-288", "label-328");
        try
        {
            item.MoveTo(ScheduledPayslipTemplateStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayslipTemplateStatus.InReview, "status");
    }

    public static void ScheduledPayslipTemplateRejectsMovesFromArchived()
    {
        var item = ScheduledPayslipTemplate.NewScheduledPayslipTemplate("code-288", "label-328");
        item.Status = ScheduledPayslipTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledPayslipTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledPayslipTemplateStartsInOpen();
        ScheduledPayslipTemplateMovesToInReview();
        ScheduledPayslipTemplateRejectsMovesFromArchived();
    }
}
