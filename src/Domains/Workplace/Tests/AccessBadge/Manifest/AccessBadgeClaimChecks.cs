namespace Acme.Workforce.Workplace.Tests.AccessBadge.Manifest;

using Acme.Workforce.Workplace.AccessBadge.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccessBadgeClaimChecks
{
    public static void AccessBadgeClaimStartsInCreated()
    {
        var item = AccessBadgeClaim.NewAccessBadgeClaim("code-256", "name-962");
        Check.Equal(item.Status, AccessBadgeClaimStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeClaimMovesToPublished()
    {
        var item = AccessBadgeClaim.NewAccessBadgeClaim("code-256", "name-962");
        try
        {
            item.MoveTo(AccessBadgeClaimStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeClaimStatus.Published, "status");
    }

    public static void AccessBadgeClaimRejectsMovesFromRetired()
    {
        var item = AccessBadgeClaim.NewAccessBadgeClaim("code-256", "name-962");
        item.Status = AccessBadgeClaimStatus.Retired;
        Check.Fails(() => item.MoveTo(AccessBadgeClaimStatus.Created), "move");
    }

    public static void RunAll()
    {
        AccessBadgeClaimStartsInCreated();
        AccessBadgeClaimMovesToPublished();
        AccessBadgeClaimRejectsMovesFromRetired();
    }
}
