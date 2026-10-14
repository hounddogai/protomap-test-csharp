namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkAssignment;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

/// <summary>Checks of work assignment code, run by the project's runner.</summary>
public static class CappedStatementOfWorkAssignmentChecks
{
    public static void CappedStatementOfWorkAssignmentStartsInOpen()
    {
        var item = CappedStatementOfWorkAssignment.NewCappedStatementOfWorkAssignment("code-958", "name-197");
        Check.Equal(item.Status, CappedStatementOfWorkAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedStatementOfWorkAssignmentMovesToInReview()
    {
        var item = CappedStatementOfWorkAssignment.NewCappedStatementOfWorkAssignment("code-958", "name-197");
        try
        {
            item.MoveTo(CappedStatementOfWorkAssignmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedStatementOfWorkAssignmentStatus.InReview, "status");
    }

    public static void CappedStatementOfWorkAssignmentRejectsMovesFromArchived()
    {
        var item = CappedStatementOfWorkAssignment.NewCappedStatementOfWorkAssignment("code-958", "name-197");
        item.Status = CappedStatementOfWorkAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(CappedStatementOfWorkAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkAssignmentStartsInOpen();
        CappedStatementOfWorkAssignmentMovesToInReview();
        CappedStatementOfWorkAssignmentRejectsMovesFromArchived();
    }
}
