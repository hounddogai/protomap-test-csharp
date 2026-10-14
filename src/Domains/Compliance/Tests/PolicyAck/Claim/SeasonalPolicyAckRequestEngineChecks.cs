namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SeasonalPolicyAckRequestEngineChecks
{
    public static void SeasonalPolicyAckRequestEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPolicyAckRequestEngine.DefaultSeasonalPolicyAckRequestEngine();
        var item = SeasonalPolicyAckRequest.NewSeasonalPolicyAckRequest("code-968", "label-120");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPolicyAckRequestEngineKeepsResultsWithinBounds();
    }
}
