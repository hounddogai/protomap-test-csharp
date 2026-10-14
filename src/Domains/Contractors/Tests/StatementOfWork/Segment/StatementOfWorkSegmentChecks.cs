namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Segment;

using Acme.Workforce.Contractors.StatementOfWork.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StatementOfWorkSegmentChecks
{
    public static void StatementOfWorkSegmentStartsInPlanned()
    {
        var item = StatementOfWorkSegment.NewStatementOfWorkSegment("code-116", "name-24");
        Check.Equal(item.Status, StatementOfWorkSegmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkSegmentMovesToCancelled()
    {
        var item = StatementOfWorkSegment.NewStatementOfWorkSegment("code-116", "name-24");
        try
        {
            item.MoveTo(StatementOfWorkSegmentStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkSegmentStatus.Cancelled, "status");
    }

    public static void StatementOfWorkSegmentRejectsMovesFromCancelled()
    {
        var item = StatementOfWorkSegment.NewStatementOfWorkSegment("code-116", "name-24");
        item.Status = StatementOfWorkSegmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StatementOfWorkSegmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkSegmentStartsInPlanned();
        StatementOfWorkSegmentMovesToCancelled();
        StatementOfWorkSegmentRejectsMovesFromCancelled();
    }
}
