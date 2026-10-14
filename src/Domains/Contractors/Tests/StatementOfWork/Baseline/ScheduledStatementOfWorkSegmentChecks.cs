namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkSegmentChecks
{
    public static void ScheduledStatementOfWorkSegmentStartsInOpen()
    {
        var item = ScheduledStatementOfWorkSegment.NewScheduledStatementOfWorkSegment("code-861", "name-956");
        Check.Equal(item.Status, ScheduledStatementOfWorkSegmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledStatementOfWorkSegmentMovesToInReview()
    {
        var item = ScheduledStatementOfWorkSegment.NewScheduledStatementOfWorkSegment("code-861", "name-956");
        try
        {
            item.MoveTo(ScheduledStatementOfWorkSegmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledStatementOfWorkSegmentStatus.InReview, "status");
    }

    public static void ScheduledStatementOfWorkSegmentRejectsMovesFromArchived()
    {
        var item = ScheduledStatementOfWorkSegment.NewScheduledStatementOfWorkSegment("code-861", "name-956");
        item.Status = ScheduledStatementOfWorkSegmentStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledStatementOfWorkSegmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkSegmentStartsInOpen();
        ScheduledStatementOfWorkSegmentMovesToInReview();
        ScheduledStatementOfWorkSegmentRejectsMovesFromArchived();
    }
}
