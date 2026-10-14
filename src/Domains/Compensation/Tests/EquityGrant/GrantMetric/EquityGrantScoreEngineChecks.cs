namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantMetric;

using Acme.Workforce.Compensation.EquityGrant.GrantMetric;

/// <summary>Checks of grant metric code, run by the project's runner.</summary>
public static class EquityGrantScoreEngineChecks
{
    public static void EquityGrantScoreEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantScoreEngine.DefaultEquityGrantScoreEngine();
        var item = EquityGrantScore.NewEquityGrantScore("code-671", "name-682");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantScoreEngineKeepsResultsWithinBounds();
    }
}
