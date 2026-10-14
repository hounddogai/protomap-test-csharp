namespace Acme.Workforce.Talent.Tests.Nomination.Target;

using Acme.Workforce.Talent.Nomination.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class NominationTargetEngineChecks
{
    public static void NominationTargetEngineKeepsResultsWithinBounds()
    {
        var engine = NominationTargetEngine.DefaultNominationTargetEngine();
        var item = NominationTarget.NewNominationTarget("code-512", "name-19");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationTargetEngineKeepsResultsWithinBounds();
    }
}
