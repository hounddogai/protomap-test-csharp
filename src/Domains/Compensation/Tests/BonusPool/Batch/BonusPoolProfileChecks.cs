namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolProfileChecks
{
    public static void BonusPoolProfileStartsInPlanned()
    {
        var item = BonusPoolProfile.NewBonusPoolProfile("code-273", "name-914");
        Check.Equal(item.Status, BonusPoolProfileStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolProfileMovesToScheduled()
    {
        var item = BonusPoolProfile.NewBonusPoolProfile("code-273", "name-914");
        try
        {
            item.MoveTo(BonusPoolProfileStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolProfileStatus.Scheduled, "status");
    }

    public static void BonusPoolProfileRejectsMovesFromCancelled()
    {
        var item = BonusPoolProfile.NewBonusPoolProfile("code-273", "name-914");
        item.Status = BonusPoolProfileStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BonusPoolProfileStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BonusPoolProfileStartsInPlanned();
        BonusPoolProfileMovesToScheduled();
        BonusPoolProfileRejectsMovesFromCancelled();
    }
}
