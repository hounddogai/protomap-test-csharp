namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantClaimEngineChecks
{
    public static void EquityGrantClaimEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantClaimEngine.DefaultEquityGrantClaimEngine();
        var item = EquityGrantClaim.NewEquityGrantClaim("code-283", "label-389");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantClaimEngineKeepsResultsWithinBounds();
    }
}
