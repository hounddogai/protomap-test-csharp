namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalPayslipNoticeEngineChecks
{
    public static void GlobalPayslipNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPayslipNoticeEngine.DefaultGlobalPayslipNoticeEngine(Flags.NewRegistry());
        var item = GlobalPayslipNotice.NewGlobalPayslipNotice("code-807", "name-390");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPayslipNoticeEngineKeepsResultsWithinBounds();
    }
}
