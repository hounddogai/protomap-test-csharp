namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalPayslipNoticeChecks
{
    public static void GlobalPayslipNoticeStartsInOpen()
    {
        var item = GlobalPayslipNotice.NewGlobalPayslipNotice("code-807", "name-390");
        Check.Equal(item.Status, GlobalPayslipNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPayslipNoticeMovesToArchived()
    {
        var item = GlobalPayslipNotice.NewGlobalPayslipNotice("code-807", "name-390");
        try
        {
            item.MoveTo(GlobalPayslipNoticeStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPayslipNoticeStatus.Archived, "status");
    }

    public static void GlobalPayslipNoticeRejectsMovesFromArchived()
    {
        var item = GlobalPayslipNotice.NewGlobalPayslipNotice("code-807", "name-390");
        item.Status = GlobalPayslipNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalPayslipNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalPayslipNoticeStartsInOpen();
        GlobalPayslipNoticeMovesToArchived();
        GlobalPayslipNoticeRejectsMovesFromArchived();
    }
}
