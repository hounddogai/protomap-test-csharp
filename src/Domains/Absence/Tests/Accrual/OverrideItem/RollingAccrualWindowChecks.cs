namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RollingAccrualWindowChecks
{
    public static void RollingAccrualWindowStartsInRequested()
    {
        var item = RollingAccrualWindow.NewRollingAccrualWindow("code-645", "title-187");
        Check.Equal(item.Status, RollingAccrualWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAccrualWindowMovesToRevoked()
    {
        var item = RollingAccrualWindow.NewRollingAccrualWindow("code-645", "title-187");
        try
        {
            item.MoveTo(RollingAccrualWindowStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAccrualWindowStatus.Revoked, "status");
    }

    public static void RollingAccrualWindowRejectsMovesFromExpired()
    {
        var item = RollingAccrualWindow.NewRollingAccrualWindow("code-645", "title-187");
        item.Status = RollingAccrualWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(RollingAccrualWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RollingAccrualWindowStartsInRequested();
        RollingAccrualWindowMovesToRevoked();
        RollingAccrualWindowRejectsMovesFromExpired();
    }
}
