namespace Acme.Workforce.Talent.Tests.Nomination.Assignment;

using Acme.Workforce.Talent.Nomination.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientNominationRequestEngineChecks
{
    public static void LenientNominationRequestEngineKeepsResultsWithinBounds()
    {
        var engine = LenientNominationRequestEngine.DefaultLenientNominationRequestEngine();
        var item = LenientNominationRequest.NewLenientNominationRequest("code-215", "label-56");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientNominationRequestEngineKeepsResultsWithinBounds();
    }
}
