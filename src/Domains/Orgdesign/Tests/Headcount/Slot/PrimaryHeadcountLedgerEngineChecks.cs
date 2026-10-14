namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PrimaryHeadcountLedgerEngineChecks
{
    public static void PrimaryHeadcountLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryHeadcountLedgerEngine.DefaultPrimaryHeadcountLedgerEngine();
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("code-124", "title-19");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryHeadcountLedgerEngineKeepsResultsWithinBounds();
    }
}
