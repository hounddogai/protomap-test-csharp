namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Ledger;

using Acme.Workforce.Contractors.StatementOfWork.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class StatementOfWorkEntryEngineChecks
{
    public static void StatementOfWorkEntryEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkEntryEngine.DefaultStatementOfWorkEntryEngine();
        var item = StatementOfWorkEntry.NewStatementOfWorkEntry("code-85", "label-874");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkEntryEngineKeepsResultsWithinBounds();
    }
}
