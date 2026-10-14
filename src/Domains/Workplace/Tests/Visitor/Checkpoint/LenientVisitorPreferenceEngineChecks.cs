namespace Acme.Workforce.Workplace.Tests.Visitor.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LenientVisitorPreferenceEngineChecks
{
    public static void LenientVisitorPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = LenientVisitorPreferenceEngine.DefaultLenientVisitorPreferenceEngine(Flags.NewRegistry());
        var item = LenientVisitorPreference.NewLenientVisitorPreference("code-739", "label-558");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientVisitorPreferenceEngineKeepsResultsWithinBounds();
    }
}
