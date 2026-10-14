namespace Acme.Workforce.Workplace.Tests.AccessBadge.Batch;

using Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccessBadgeBatchChecks
{
    public static void AccessBadgeBatchStartsInOpen()
    {
        var item = AccessBadgeBatch.NewAccessBadgeBatch("code-739", "name-687");
        Check.Equal(item.Status, AccessBadgeBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeBatchMovesToInReview()
    {
        var item = AccessBadgeBatch.NewAccessBadgeBatch("code-739", "name-687");
        try
        {
            item.MoveTo(AccessBadgeBatchStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeBatchStatus.InReview, "status");
    }

    public static void AccessBadgeBatchRejectsMovesFromArchived()
    {
        var item = AccessBadgeBatch.NewAccessBadgeBatch("code-739", "name-687");
        item.Status = AccessBadgeBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(AccessBadgeBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        AccessBadgeBatchStartsInOpen();
        AccessBadgeBatchMovesToInReview();
        AccessBadgeBatchRejectsMovesFromArchived();
    }
}
