namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolWindowChecks
{
    public static void BonusPoolWindowStartsInRequested()
    {
        var item = BonusPoolWindow.NewBonusPoolWindow("code-909", "name-112");
        Check.Equal(item.Status, BonusPoolWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolWindowMovesToRevoked()
    {
        var item = BonusPoolWindow.NewBonusPoolWindow("code-909", "name-112");
        try
        {
            item.MoveTo(BonusPoolWindowStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolWindowStatus.Revoked, "status");
    }

    public static void BonusPoolWindowRejectsMovesFromExpired()
    {
        var item = BonusPoolWindow.NewBonusPoolWindow("code-909", "name-112");
        item.Status = BonusPoolWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(BonusPoolWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BonusPoolWindowStartsInRequested();
        BonusPoolWindowMovesToRevoked();
        BonusPoolWindowRejectsMovesFromExpired();
    }
}
