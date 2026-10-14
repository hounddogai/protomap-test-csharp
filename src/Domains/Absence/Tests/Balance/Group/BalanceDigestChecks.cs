namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceDigestChecks
{
    public static void BalanceDigestStartsInPlanned()
    {
        var item = BalanceDigest.NewBalanceDigest("code-503", "title-284");
        Check.Equal(item.Status, BalanceDigestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceDigestMovesToInProgress()
    {
        var item = BalanceDigest.NewBalanceDigest("code-503", "title-284");
        try
        {
            item.MoveTo(BalanceDigestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceDigestStatus.InProgress, "status");
    }

    public static void BalanceDigestRejectsMovesFromCancelled()
    {
        var item = BalanceDigest.NewBalanceDigest("code-503", "title-284");
        item.Status = BalanceDigestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BalanceDigestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BalanceDigestStartsInPlanned();
        BalanceDigestMovesToInProgress();
        BalanceDigestRejectsMovesFromCancelled();
    }
}
