namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeDigestChecks
{
    public static void AccessBadgeDigestStartsInProposed()
    {
        var item = AccessBadgeDigest.NewAccessBadgeDigest("code-101", "name-800");
        Check.Equal(item.Status, AccessBadgeDigestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeDigestMovesToApproved()
    {
        var item = AccessBadgeDigest.NewAccessBadgeDigest("code-101", "name-800");
        try
        {
            item.MoveTo(AccessBadgeDigestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeDigestStatus.Approved, "status");
    }

    public static void AccessBadgeDigestRejectsMovesFromWithdrawn()
    {
        var item = AccessBadgeDigest.NewAccessBadgeDigest("code-101", "name-800");
        item.Status = AccessBadgeDigestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AccessBadgeDigestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AccessBadgeDigestStartsInProposed();
        AccessBadgeDigestMovesToApproved();
        AccessBadgeDigestRejectsMovesFromWithdrawn();
    }
}
