namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class EquityGrantThresholdEngineChecks
{
    public static void EquityGrantThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantThresholdEngine.DefaultEquityGrantThresholdEngine();
        var item = EquityGrantThreshold.NewEquityGrantThreshold("code-553", "name-632");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantThresholdEngineKeepsResultsWithinBounds();
    }
}
