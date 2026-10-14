namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedStatementOfWorkAssignmentChecks
{
    public static void WeightedStatementOfWorkAssignmentStartsInRequested()
    {
        var item = WeightedStatementOfWorkAssignment.NewWeightedStatementOfWorkAssignment("code-951", "label-10");
        Check.Equal(item.Status, WeightedStatementOfWorkAssignmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedStatementOfWorkAssignmentMovesToExpired()
    {
        var item = WeightedStatementOfWorkAssignment.NewWeightedStatementOfWorkAssignment("code-951", "label-10");
        try
        {
            item.MoveTo(WeightedStatementOfWorkAssignmentStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedStatementOfWorkAssignmentStatus.Expired, "status");
    }

    public static void WeightedStatementOfWorkAssignmentRejectsMovesFromExpired()
    {
        var item = WeightedStatementOfWorkAssignment.NewWeightedStatementOfWorkAssignment("code-951", "label-10");
        item.Status = WeightedStatementOfWorkAssignmentStatus.Expired;
        Check.Fails(() => item.MoveTo(WeightedStatementOfWorkAssignmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WeightedStatementOfWorkAssignmentStartsInRequested();
        WeightedStatementOfWorkAssignmentMovesToExpired();
        WeightedStatementOfWorkAssignmentRejectsMovesFromExpired();
    }
}
