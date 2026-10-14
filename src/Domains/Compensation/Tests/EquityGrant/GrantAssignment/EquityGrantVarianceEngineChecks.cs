namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantAssignment;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Checks of grant assignment code, run by the project's runner.</summary>
public static class EquityGrantVarianceEngineChecks
{
    public static void EquityGrantVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantVarianceEngine.DefaultEquityGrantVarianceEngine();
        var item = EquityGrantVariance.NewEquityGrantVariance("code-615", "title-587");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantVarianceEngineKeepsResultsWithinBounds();
    }
}
