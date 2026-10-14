namespace Acme.Workforce.Talent.Tests.Mobility.MobilityReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.MobilityReservation;

/// <summary>Checks of mobility reservation code, run by the project's runner.</summary>
public static class CappedMobilityOverrideEngineChecks
{
    public static void CappedMobilityOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = CappedMobilityOverrideEngine.DefaultCappedMobilityOverrideEngine(Flags.NewRegistry());
        var item = CappedMobilityOverride.NewCappedMobilityOverride("code-592", "title-148");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedMobilityOverrideEngineKeepsResultsWithinBounds();
    }
}
