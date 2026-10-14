namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FallbackStatementOfWorkSegmentChecks
{
    public static void FallbackStatementOfWorkSegmentStartsInIdle()
    {
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("code-187", "name-726");
        Check.Equal(item.Status, FallbackStatementOfWorkSegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackStatementOfWorkSegmentMovesToWarming()
    {
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("code-187", "name-726");
        try
        {
            item.MoveTo(FallbackStatementOfWorkSegmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackStatementOfWorkSegmentStatus.Warming, "status");
    }

    public static void FallbackStatementOfWorkSegmentRejectsMovesFromStopped()
    {
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("code-187", "name-726");
        item.Status = FallbackStatementOfWorkSegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackStatementOfWorkSegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackStatementOfWorkSegmentStartsInIdle();
        FallbackStatementOfWorkSegmentMovesToWarming();
        FallbackStatementOfWorkSegmentRejectsMovesFromStopped();
    }
}
