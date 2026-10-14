namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkAssignmentEngineChecks
{
    public static void StatementOfWorkAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkAssignmentEngine.DefaultStatementOfWorkAssignmentEngine(Flags.NewRegistry());
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("code-748", "name-75");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkAssignmentEngineKeepsResultsWithinBounds();
    }
}
