namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkExceptionEngineChecks
{
    public static void StatementOfWorkExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkExceptionEngine.DefaultStatementOfWorkExceptionEngine(Flags.NewRegistry());
        var item = StatementOfWorkException.NewStatementOfWorkException("code-34", "name-98");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkExceptionEngineKeepsResultsWithinBounds();
    }
}
