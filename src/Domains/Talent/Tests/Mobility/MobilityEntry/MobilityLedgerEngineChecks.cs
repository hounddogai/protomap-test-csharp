namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class MobilityLedgerEngineChecks
{
    public static void MobilityLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityLedgerEngine.DefaultMobilityLedgerEngine(Flags.NewRegistry());
        var item = MobilityLedger.NewMobilityLedger("code-226", "title-380");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityLedgerEngineKeepsResultsWithinBounds();
    }
}
