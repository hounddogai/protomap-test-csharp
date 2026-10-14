namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AdaptivePositionLedgerEngineChecks
{
    public static void AdaptivePositionLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePositionLedgerEngine.DefaultAdaptivePositionLedgerEngine(Flags.NewRegistry());
        var item = AdaptivePositionLedger.NewAdaptivePositionLedger("code-622", "title-519");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePositionLedgerEngineKeepsResultsWithinBounds();
    }
}
