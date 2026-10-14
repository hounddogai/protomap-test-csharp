namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class StrictPolicyEntryEngineChecks
{
    public static void StrictPolicyEntryEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPolicyEntryEngine.DefaultStrictPolicyEntryEngine(Flags.NewRegistry());
        var item = StrictPolicyEntry.NewStrictPolicyEntry("code-352", "name-282");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPolicyEntryEngineKeepsResultsWithinBounds();
    }
}
