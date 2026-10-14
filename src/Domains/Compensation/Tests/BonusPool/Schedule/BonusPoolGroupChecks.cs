namespace Acme.Workforce.Compensation.Tests.BonusPool.Schedule;

using Acme.Workforce.Compensation.BonusPool.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BonusPoolGroupChecks
{
    public static void BonusPoolGroupStartsInPlanned()
    {
        var item = BonusPoolGroup.NewBonusPoolGroup("code-741", "label-824");
        Check.Equal(item.Status, BonusPoolGroupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolGroupMovesToInProgress()
    {
        var item = BonusPoolGroup.NewBonusPoolGroup("code-741", "label-824");
        try
        {
            item.MoveTo(BonusPoolGroupStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolGroupStatus.InProgress, "status");
    }

    public static void BonusPoolGroupRejectsMovesFromCancelled()
    {
        var item = BonusPoolGroup.NewBonusPoolGroup("code-741", "label-824");
        item.Status = BonusPoolGroupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BonusPoolGroupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BonusPoolGroupStartsInPlanned();
        BonusPoolGroupMovesToInProgress();
        BonusPoolGroupRejectsMovesFromCancelled();
    }
}
