namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkAssignment;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of work assignment code, run by the project's runner.</summary>
public static class CappedStatementOfWorkAssignmentEngineChecks
{
    public static void CappedStatementOfWorkAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = CappedStatementOfWorkAssignmentEngine.DefaultCappedStatementOfWorkAssignmentEngine(Flags.NewRegistry());
        var item = CappedStatementOfWorkAssignment.NewCappedStatementOfWorkAssignment("code-958", "name-197");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkAssignmentEngineKeepsResultsWithinBounds();
    }
}
