namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BatchedAccessBadgeMappingChecks
{
    public static void BatchedAccessBadgeMappingStartsInRequested()
    {
        var item = BatchedAccessBadgeMapping.NewBatchedAccessBadgeMapping("code-977", "label-599");
        Check.Equal(item.Status, BatchedAccessBadgeMappingStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAccessBadgeMappingMovesToGranted()
    {
        var item = BatchedAccessBadgeMapping.NewBatchedAccessBadgeMapping("code-977", "label-599");
        try
        {
            item.MoveTo(BatchedAccessBadgeMappingStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAccessBadgeMappingStatus.Granted, "status");
    }

    public static void BatchedAccessBadgeMappingRejectsMovesFromExpired()
    {
        var item = BatchedAccessBadgeMapping.NewBatchedAccessBadgeMapping("code-977", "label-599");
        item.Status = BatchedAccessBadgeMappingStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedAccessBadgeMappingStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedAccessBadgeMappingStartsInRequested();
        BatchedAccessBadgeMappingMovesToGranted();
        BatchedAccessBadgeMappingRejectsMovesFromExpired();
    }
}
