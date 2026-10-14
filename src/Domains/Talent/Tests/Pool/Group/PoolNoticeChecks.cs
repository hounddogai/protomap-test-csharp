namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PoolNoticeChecks
{
    public static void PoolNoticeStartsInProposed()
    {
        var item = PoolNotice.NewPoolNotice("code-407", "label-999");
        Check.Equal(item.Status, PoolNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolNoticeMovesToRejected()
    {
        var item = PoolNotice.NewPoolNotice("code-407", "label-999");
        try
        {
            item.MoveTo(PoolNoticeStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolNoticeStatus.Rejected, "status");
    }

    public static void PoolNoticeRejectsMovesFromWithdrawn()
    {
        var item = PoolNotice.NewPoolNotice("code-407", "label-999");
        item.Status = PoolNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PoolNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PoolNoticeStartsInProposed();
        PoolNoticeMovesToRejected();
        PoolNoticeRejectsMovesFromWithdrawn();
    }
}
