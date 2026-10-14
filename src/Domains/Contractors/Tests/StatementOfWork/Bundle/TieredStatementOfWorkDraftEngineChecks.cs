namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredStatementOfWorkDraftEngineChecks
{
    public static void TieredStatementOfWorkDraftEngineKeepsResultsWithinBounds()
    {
        var engine = TieredStatementOfWorkDraftEngine.DefaultTieredStatementOfWorkDraftEngine(Flags.NewRegistry());
        var item = TieredStatementOfWorkDraft.NewTieredStatementOfWorkDraft("code-164", "name-678");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredStatementOfWorkDraftEngineKeepsResultsWithinBounds();
    }
}
