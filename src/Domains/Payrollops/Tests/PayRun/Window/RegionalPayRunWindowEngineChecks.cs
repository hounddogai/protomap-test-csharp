namespace Acme.Workforce.Payrollops.Tests.PayRun.Window;

using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalPayRunWindowEngineChecks
{
    public static void RegionalPayRunWindowEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalPayRunWindowEngine.DefaultRegionalPayRunWindowEngine();
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("code-496", "label-294");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalPayRunWindowEngineKeepsResultsWithinBounds();
    }
}
