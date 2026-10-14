namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class PendingCareerPathBatchChecks
{
    public static void PendingCareerPathBatchStartsInPlanned()
    {
        var item = PendingCareerPathBatch.NewPendingCareerPathBatch("code-789", "title-46");
        Check.Equal(item.Status, PendingCareerPathBatchStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingCareerPathBatchMovesToInProgress()
    {
        var item = PendingCareerPathBatch.NewPendingCareerPathBatch("code-789", "title-46");
        try
        {
            item.MoveTo(PendingCareerPathBatchStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingCareerPathBatchStatus.InProgress, "status");
    }

    public static void PendingCareerPathBatchRejectsMovesFromCancelled()
    {
        var item = PendingCareerPathBatch.NewPendingCareerPathBatch("code-789", "title-46");
        item.Status = PendingCareerPathBatchStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingCareerPathBatchStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingCareerPathBatchStartsInPlanned();
        PendingCareerPathBatchMovesToInProgress();
        PendingCareerPathBatchRejectsMovesFromCancelled();
    }
}
