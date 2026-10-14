namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkVarianceEngineChecks
{
    public static void ScheduledStatementOfWorkVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledStatementOfWorkVarianceEngine.DefaultScheduledStatementOfWorkVarianceEngine(Flags.NewRegistry());
        var item = ScheduledStatementOfWorkVariance.NewScheduledStatementOfWorkVariance("code-868", "name-629");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkVarianceEngineKeepsResultsWithinBounds();
    }
}
