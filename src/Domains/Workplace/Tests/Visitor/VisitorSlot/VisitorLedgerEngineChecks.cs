namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class VisitorLedgerEngineChecks
{
    public static void VisitorLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorLedgerEngine.DefaultVisitorLedgerEngine();
        var item = VisitorLedger.NewVisitorLedger("code-295", "title-921");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorLedgerEngineKeepsResultsWithinBounds();
    }
}
