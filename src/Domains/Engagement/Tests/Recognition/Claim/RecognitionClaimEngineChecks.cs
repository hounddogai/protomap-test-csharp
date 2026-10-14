namespace Acme.Workforce.Engagement.Tests.Recognition.Claim;

using Acme.Workforce.Engagement.Recognition.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RecognitionClaimEngineChecks
{
    public static void RecognitionClaimEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionClaimEngine.DefaultRecognitionClaimEngine();
        var item = RecognitionClaim.NewRecognitionClaim("code-755", "name-368");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionClaimEngineKeepsResultsWithinBounds();
    }
}
