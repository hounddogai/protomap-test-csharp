namespace Acme.Workforce.Compensation.Tests.EquityGrant.Tier;

using Acme.Workforce.Compensation.EquityGrant.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EquityGrantWindowEngineChecks
{
    public static void EquityGrantWindowEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantWindowEngine.DefaultEquityGrantWindowEngine();
        var item = EquityGrantWindow.NewEquityGrantWindow("code-734", "label-585");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantWindowEngineKeepsResultsWithinBounds();
    }
}
