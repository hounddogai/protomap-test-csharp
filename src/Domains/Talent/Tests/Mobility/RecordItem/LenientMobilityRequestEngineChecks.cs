namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientMobilityRequestEngineChecks
{
    public static void LenientMobilityRequestEngineKeepsResultsWithinBounds()
    {
        var engine = LenientMobilityRequestEngine.DefaultLenientMobilityRequestEngine(Flags.NewRegistry());
        var item = LenientMobilityRequest.NewLenientMobilityRequest("code-775", "name-923");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientMobilityRequestEngineKeepsResultsWithinBounds();
    }
}
