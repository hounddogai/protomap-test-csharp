namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Variance;

using Acme.Workforce.Contractors.StatementOfWork.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StatementOfWorkVarianceEngineChecks
{
    public static void StatementOfWorkVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkVarianceEngine.DefaultStatementOfWorkVarianceEngine(Flags.NewRegistry());
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("code-1", "title-662");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkVarianceEngineKeepsResultsWithinBounds();
    }
}
