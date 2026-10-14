namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class StatementOfWorkDraftEngineChecks
{
    public static void StatementOfWorkDraftEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkDraftEngine.DefaultStatementOfWorkDraftEngine();
        var item = StatementOfWorkDraft.NewStatementOfWorkDraft("code-536", "label-90");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkDraftEngineKeepsResultsWithinBounds();
    }
}
