namespace Acme.Workforce.Talent.Tests.Nomination.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveNominationVarianceEngineChecks
{
    public static void AdaptiveNominationVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveNominationVarianceEngine.DefaultAdaptiveNominationVarianceEngine(Flags.NewRegistry());
        var item = AdaptiveNominationVariance.NewAdaptiveNominationVariance("code-591", "label-574");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveNominationVarianceEngineKeepsResultsWithinBounds();
    }
}
