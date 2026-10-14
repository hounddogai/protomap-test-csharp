namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class PayBandPolicyEngineChecks
{
    public static void PayBandPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandPolicyEngine.DefaultPayBandPolicyEngine();
        var item = PayBandPolicy.NewPayBandPolicy("code-608", "title-306");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandPolicyEngineKeepsResultsWithinBounds();
    }
}
