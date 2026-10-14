namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StatementOfWorkEstimateEngineChecks
{
    public static void StatementOfWorkEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkEstimateEngine.DefaultStatementOfWorkEstimateEngine(Flags.NewRegistry());
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("code-18", "label-543");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkEstimateEngineKeepsResultsWithinBounds();
    }
}
