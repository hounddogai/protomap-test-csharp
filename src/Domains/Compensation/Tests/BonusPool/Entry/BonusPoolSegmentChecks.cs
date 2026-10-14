namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolSegmentChecks
{
    public static void BonusPoolSegmentStartsInNew()
    {
        var item = BonusPoolSegment.NewBonusPoolSegment("code-848", "label-929");
        Check.Equal(item.Status, BonusPoolSegmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolSegmentMovesToQueued()
    {
        var item = BonusPoolSegment.NewBonusPoolSegment("code-848", "label-929");
        try
        {
            item.MoveTo(BonusPoolSegmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolSegmentStatus.Queued, "status");
    }

    public static void BonusPoolSegmentRejectsMovesFromFailed()
    {
        var item = BonusPoolSegment.NewBonusPoolSegment("code-848", "label-929");
        item.Status = BonusPoolSegmentStatus.Failed;
        Check.Fails(() => item.MoveTo(BonusPoolSegmentStatus.New), "move");
    }

    public static void RunAll()
    {
        BonusPoolSegmentStartsInNew();
        BonusPoolSegmentMovesToQueued();
        BonusPoolSegmentRejectsMovesFromFailed();
    }
}
