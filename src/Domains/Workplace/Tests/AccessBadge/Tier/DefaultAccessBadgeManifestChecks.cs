namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class DefaultAccessBadgeManifestChecks
{
    public static void DefaultAccessBadgeManifestStartsInCreated()
    {
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("code-720", "label-694");
        Check.Equal(item.Status, DefaultAccessBadgeManifestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAccessBadgeManifestMovesToPublished()
    {
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("code-720", "label-694");
        try
        {
            item.MoveTo(DefaultAccessBadgeManifestStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAccessBadgeManifestStatus.Published, "status");
    }

    public static void DefaultAccessBadgeManifestRejectsMovesFromRetired()
    {
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("code-720", "label-694");
        item.Status = DefaultAccessBadgeManifestStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultAccessBadgeManifestStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeManifestStartsInCreated();
        DefaultAccessBadgeManifestMovesToPublished();
        DefaultAccessBadgeManifestRejectsMovesFromRetired();
    }
}
