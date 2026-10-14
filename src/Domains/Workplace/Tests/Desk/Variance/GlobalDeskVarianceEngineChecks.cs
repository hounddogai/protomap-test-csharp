namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GlobalDeskVarianceEngineChecks
{
    public static void GlobalDeskVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalDeskVarianceEngine.DefaultGlobalDeskVarianceEngine();
        var item = GlobalDeskVariance.NewGlobalDeskVariance("code-882", "label-728");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalDeskVarianceEngineKeepsResultsWithinBounds();
    }
}
