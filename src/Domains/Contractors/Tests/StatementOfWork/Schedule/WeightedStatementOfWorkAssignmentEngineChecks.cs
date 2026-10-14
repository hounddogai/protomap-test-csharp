namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedStatementOfWorkAssignmentEngineChecks
{
    public static void WeightedStatementOfWorkAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedStatementOfWorkAssignmentEngine.DefaultWeightedStatementOfWorkAssignmentEngine(Flags.NewRegistry());
        var item = WeightedStatementOfWorkAssignment.NewWeightedStatementOfWorkAssignment("code-951", "label-10");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedStatementOfWorkAssignmentEngineKeepsResultsWithinBounds();
    }
}
