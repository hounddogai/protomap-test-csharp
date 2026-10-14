namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkAssignmentChecks
{
    public static void StatementOfWorkAssignmentStartsInCreated()
    {
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("code-748", "name-75");
        Check.Equal(item.Status, StatementOfWorkAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkAssignmentMovesToValidated()
    {
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("code-748", "name-75");
        try
        {
            item.MoveTo(StatementOfWorkAssignmentStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkAssignmentStatus.Validated, "status");
    }

    public static void StatementOfWorkAssignmentRejectsMovesFromRetired()
    {
        var item = StatementOfWorkAssignment.NewStatementOfWorkAssignment("code-748", "name-75");
        item.Status = StatementOfWorkAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(StatementOfWorkAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkAssignmentStartsInCreated();
        StatementOfWorkAssignmentMovesToValidated();
        StatementOfWorkAssignmentRejectsMovesFromRetired();
    }
}
