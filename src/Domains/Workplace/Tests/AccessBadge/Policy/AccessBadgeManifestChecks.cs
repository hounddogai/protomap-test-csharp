namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AccessBadgeManifestChecks
{
    public static void AccessBadgeManifestStartsInRequested()
    {
        var item = AccessBadgeManifest.NewAccessBadgeManifest("code-997", "label-157");
        Check.Equal(item.Status, AccessBadgeManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeManifestMovesToRevoked()
    {
        var item = AccessBadgeManifest.NewAccessBadgeManifest("code-997", "label-157");
        try
        {
            item.MoveTo(AccessBadgeManifestStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeManifestStatus.Revoked, "status");
    }

    public static void AccessBadgeManifestRejectsMovesFromExpired()
    {
        var item = AccessBadgeManifest.NewAccessBadgeManifest("code-997", "label-157");
        item.Status = AccessBadgeManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(AccessBadgeManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AccessBadgeManifestStartsInRequested();
        AccessBadgeManifestMovesToRevoked();
        AccessBadgeManifestRejectsMovesFromExpired();
    }
}
