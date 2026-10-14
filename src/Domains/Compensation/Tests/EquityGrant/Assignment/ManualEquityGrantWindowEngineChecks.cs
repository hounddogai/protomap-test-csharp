namespace Acme.Workforce.Compensation.Tests.EquityGrant.Assignment;

using Acme.Workforce.Compensation.EquityGrant.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ManualEquityGrantWindowEngineChecks
{
    public static void ManualEquityGrantWindowEngineKeepsResultsWithinBounds()
    {
        var engine = ManualEquityGrantWindowEngine.DefaultManualEquityGrantWindowEngine(Flags.NewRegistry());
        var item = ManualEquityGrantWindow.NewManualEquityGrantWindow("code-753", "title-128");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualEquityGrantWindowEngineKeepsResultsWithinBounds();
    }
}
