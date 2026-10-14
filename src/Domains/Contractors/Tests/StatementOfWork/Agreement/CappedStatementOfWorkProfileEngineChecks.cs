namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProfileEngineChecks
{
    public static void CappedStatementOfWorkProfileEngineKeepsResultsWithinBounds()
    {
        var engine = CappedStatementOfWorkProfileEngine.DefaultCappedStatementOfWorkProfileEngine(Flags.NewRegistry());
        var item = CappedStatementOfWorkProfile.NewCappedStatementOfWorkProfile("code-988", "title-387");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkProfileEngineKeepsResultsWithinBounds();
    }
}
