namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class NominationExceptionEngineChecks
{
    public static void NominationExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = NominationExceptionEngine.DefaultNominationExceptionEngine(Flags.NewRegistry());
        var item = NominationException.NewNominationException("code-371", "name-468");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationExceptionEngineKeepsResultsWithinBounds();
    }
}
