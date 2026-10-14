namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalBalanceDigestChecks
{
    public static void RegionalBalanceDigestStartsInPlanned()
    {
        var item = RegionalBalanceDigest.NewRegionalBalanceDigest("code-294", "name-263");
        Check.Equal(item.Status, RegionalBalanceDigestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalBalanceDigestMovesToInProgress()
    {
        var item = RegionalBalanceDigest.NewRegionalBalanceDigest("code-294", "name-263");
        try
        {
            item.MoveTo(RegionalBalanceDigestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalBalanceDigestStatus.InProgress, "status");
    }

    public static void RegionalBalanceDigestRejectsMovesFromCancelled()
    {
        var item = RegionalBalanceDigest.NewRegionalBalanceDigest("code-294", "name-263");
        item.Status = RegionalBalanceDigestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalBalanceDigestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalBalanceDigestStartsInPlanned();
        RegionalBalanceDigestMovesToInProgress();
        RegionalBalanceDigestRejectsMovesFromCancelled();
    }
}
