namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationTierEngineChecks
{
    public static void NominationTierEngineKeepsResultsWithinBounds()
    {
        var engine = NominationTierEngine.DefaultNominationTierEngine(Flags.NewRegistry());
        var item = NominationTier.NewNominationTier("code-837", "label-532");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationTierEngineKeepsResultsWithinBounds();
    }
}
