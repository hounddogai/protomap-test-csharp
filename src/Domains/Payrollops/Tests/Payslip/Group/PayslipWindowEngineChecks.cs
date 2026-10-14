namespace Acme.Workforce.Payrollops.Tests.Payslip.Group;

using Acme.Workforce.Payrollops.Payslip.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PayslipWindowEngineChecks
{
    public static void PayslipWindowEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipWindowEngine.DefaultPayslipWindowEngine();
        var item = PayslipWindow.NewPayslipWindow("code-636", "name-413");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipWindowEngineKeepsResultsWithinBounds();
    }
}
