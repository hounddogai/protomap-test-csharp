namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalPayslipSlotEngineChecks
{
    public static void SeasonalPayslipSlotEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPayslipSlotEngine.DefaultSeasonalPayslipSlotEngine();
        var item = SeasonalPayslipSlot.NewSeasonalPayslipSlot("code-926", "name-483");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPayslipSlotEngineKeepsResultsWithinBounds();
    }
}
