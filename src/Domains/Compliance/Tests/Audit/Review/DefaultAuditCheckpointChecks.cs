namespace Acme.Workforce.Compliance.Tests.Audit.Review;

using Acme.Workforce.Compliance.Audit.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultAuditCheckpointChecks
{
    public static void DefaultAuditCheckpointStartsInOpen()
    {
        var item = DefaultAuditCheckpoint.NewDefaultAuditCheckpoint("code-816", "name-623");
        Check.Equal(item.Status, DefaultAuditCheckpointStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAuditCheckpointMovesToInReview()
    {
        var item = DefaultAuditCheckpoint.NewDefaultAuditCheckpoint("code-816", "name-623");
        try
        {
            item.MoveTo(DefaultAuditCheckpointStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAuditCheckpointStatus.InReview, "status");
    }

    public static void DefaultAuditCheckpointRejectsMovesFromArchived()
    {
        var item = DefaultAuditCheckpoint.NewDefaultAuditCheckpoint("code-816", "name-623");
        item.Status = DefaultAuditCheckpointStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultAuditCheckpointStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultAuditCheckpointStartsInOpen();
        DefaultAuditCheckpointMovesToInReview();
        DefaultAuditCheckpointRejectsMovesFromArchived();
    }
}
