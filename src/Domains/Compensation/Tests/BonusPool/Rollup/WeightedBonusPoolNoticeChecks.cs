namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedBonusPoolNoticeChecks
{
    public static void WeightedBonusPoolNoticeStartsInOpen()
    {
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("code-855", "name-6");
        Check.Equal(item.Status, WeightedBonusPoolNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBonusPoolNoticeMovesToResolved()
    {
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("code-855", "name-6");
        try
        {
            item.MoveTo(WeightedBonusPoolNoticeStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBonusPoolNoticeStatus.Resolved, "status");
    }

    public static void WeightedBonusPoolNoticeRejectsMovesFromArchived()
    {
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("code-855", "name-6");
        item.Status = WeightedBonusPoolNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedBonusPoolNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedBonusPoolNoticeStartsInOpen();
        WeightedBonusPoolNoticeMovesToResolved();
        WeightedBonusPoolNoticeRejectsMovesFromArchived();
    }
}
