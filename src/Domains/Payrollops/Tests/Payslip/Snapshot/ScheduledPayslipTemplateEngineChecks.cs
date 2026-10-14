namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledPayslipTemplateEngineChecks
{
    public static void ScheduledPayslipTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPayslipTemplateEngine.DefaultScheduledPayslipTemplateEngine();
        var item = ScheduledPayslipTemplate.NewScheduledPayslipTemplate("code-288", "label-328");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPayslipTemplateEngineKeepsResultsWithinBounds();
    }
}
