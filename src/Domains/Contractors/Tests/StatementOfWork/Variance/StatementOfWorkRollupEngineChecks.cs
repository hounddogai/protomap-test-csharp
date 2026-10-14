namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Variance;

using Acme.Workforce.Contractors.StatementOfWork.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StatementOfWorkRollupEngineChecks
{
    public static void StatementOfWorkRollupEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkRollupEngine.DefaultStatementOfWorkRollupEngine(Flags.NewRegistry());
        var item = StatementOfWorkRollup.NewStatementOfWorkRollup("code-380", "label-820");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkRollupEngineKeepsResultsWithinBounds();
    }
}
