namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Profile;

using Acme.Workforce.Contractors.StatementOfWork.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedStatementOfWorkExceptionEngineChecks
{
    public static void CappedStatementOfWorkExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = CappedStatementOfWorkExceptionEngine.DefaultCappedStatementOfWorkExceptionEngine(Flags.NewRegistry());
        var item = CappedStatementOfWorkException.NewCappedStatementOfWorkException("code-789", "name-124");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkExceptionEngineKeepsResultsWithinBounds();
    }
}
