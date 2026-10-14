namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class NominationPreferenceEngineChecks
{
    public static void NominationPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = NominationPreferenceEngine.DefaultNominationPreferenceEngine(Flags.NewRegistry());
        var item = NominationPreference.NewNominationPreference("code-805", "name-754");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationPreferenceEngineKeepsResultsWithinBounds();
    }
}
