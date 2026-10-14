namespace Acme.Workforce.Compensation.Tests.BonusPool.Profile;

using Acme.Workforce.Compensation.BonusPool.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingBonusPoolProfileChecks
{
    public static void RollingBonusPoolProfileStartsInDraft()
    {
        var item = RollingBonusPoolProfile.NewRollingBonusPoolProfile("code-84", "name-119");
        Check.Equal(item.Status, RollingBonusPoolProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingBonusPoolProfileMovesToActive()
    {
        var item = RollingBonusPoolProfile.NewRollingBonusPoolProfile("code-84", "name-119");
        try
        {
            item.MoveTo(RollingBonusPoolProfileStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingBonusPoolProfileStatus.Active, "status");
    }

    public static void RollingBonusPoolProfileRejectsMovesFromClosed()
    {
        var item = RollingBonusPoolProfile.NewRollingBonusPoolProfile("code-84", "name-119");
        item.Status = RollingBonusPoolProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingBonusPoolProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingBonusPoolProfileStartsInDraft();
        RollingBonusPoolProfileMovesToActive();
        RollingBonusPoolProfileRejectsMovesFromClosed();
    }
}
