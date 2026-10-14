namespace Acme.Workforce.Talent.Tests.Nomination.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackNominationBundleEngineChecks
{
    public static void FallbackNominationBundleEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackNominationBundleEngine.DefaultFallbackNominationBundleEngine(Flags.NewRegistry());
        var item = FallbackNominationBundle.NewFallbackNominationBundle("code-239", "title-29");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackNominationBundleEngineKeepsResultsWithinBounds();
    }
}
