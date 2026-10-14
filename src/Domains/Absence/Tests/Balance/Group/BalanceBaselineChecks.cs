namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceBaselineChecks
{
    public static void BalanceBaselineStartsInRequested()
    {
        var item = BalanceBaseline.NewBalanceBaseline("code-445", "title-479");
        Check.Equal(item.Status, BalanceBaselineStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceBaselineMovesToExpired()
    {
        var item = BalanceBaseline.NewBalanceBaseline("code-445", "title-479");
        try
        {
            item.MoveTo(BalanceBaselineStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceBaselineStatus.Expired, "status");
    }

    public static void BalanceBaselineRejectsMovesFromExpired()
    {
        var item = BalanceBaseline.NewBalanceBaseline("code-445", "title-479");
        item.Status = BalanceBaselineStatus.Expired;
        Check.Fails(() => item.MoveTo(BalanceBaselineStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BalanceBaselineStartsInRequested();
        BalanceBaselineMovesToExpired();
        BalanceBaselineRejectsMovesFromExpired();
    }
}
