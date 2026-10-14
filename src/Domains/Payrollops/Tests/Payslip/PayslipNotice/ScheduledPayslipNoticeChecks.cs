namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class ScheduledPayslipNoticeChecks
{
    public static void ScheduledPayslipNoticeStartsInIdle()
    {
        var item = ScheduledPayslipNotice.NewScheduledPayslipNotice("code-328", "name-891");
        Check.Equal(item.Status, ScheduledPayslipNoticeStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayslipNoticeMovesToReady()
    {
        var item = ScheduledPayslipNotice.NewScheduledPayslipNotice("code-328", "name-891");
        try
        {
            item.MoveTo(ScheduledPayslipNoticeStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayslipNoticeStatus.Ready, "status");
    }

    public static void ScheduledPayslipNoticeRejectsMovesFromStopped()
    {
        var item = ScheduledPayslipNotice.NewScheduledPayslipNotice("code-328", "name-891");
        item.Status = ScheduledPayslipNoticeStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledPayslipNoticeStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledPayslipNoticeStartsInIdle();
        ScheduledPayslipNoticeMovesToReady();
        ScheduledPayslipNoticeRejectsMovesFromStopped();
    }
}
