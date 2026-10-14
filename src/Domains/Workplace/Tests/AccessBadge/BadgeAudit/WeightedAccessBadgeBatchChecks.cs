namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Checks of badge audit code, run by the project's runner.</summary>
public static class WeightedAccessBadgeBatchChecks
{
    public static void WeightedAccessBadgeBatchStartsInOpen()
    {
        var item = WeightedAccessBadgeBatch.NewWeightedAccessBadgeBatch("code-430", "label-124");
        Check.Equal(item.Status, WeightedAccessBadgeBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAccessBadgeBatchMovesToInReview()
    {
        var item = WeightedAccessBadgeBatch.NewWeightedAccessBadgeBatch("code-430", "label-124");
        try
        {
            item.MoveTo(WeightedAccessBadgeBatchStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAccessBadgeBatchStatus.InReview, "status");
    }

    public static void WeightedAccessBadgeBatchRejectsMovesFromArchived()
    {
        var item = WeightedAccessBadgeBatch.NewWeightedAccessBadgeBatch("code-430", "label-124");
        item.Status = WeightedAccessBadgeBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedAccessBadgeBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeBatchStartsInOpen();
        WeightedAccessBadgeBatchMovesToInReview();
        WeightedAccessBadgeBatchRejectsMovesFromArchived();
    }
}
