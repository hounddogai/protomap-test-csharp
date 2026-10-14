namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LenientEngagementManifestChecks
{
    public static void LenientEngagementManifestStartsInCreated()
    {
        var item = LenientEngagementManifest.NewLenientEngagementManifest("code-891", "label-522");
        Check.Equal(item.Status, LenientEngagementManifestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientEngagementManifestMovesToValidated()
    {
        var item = LenientEngagementManifest.NewLenientEngagementManifest("code-891", "label-522");
        try
        {
            item.MoveTo(LenientEngagementManifestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientEngagementManifestStatus.Validated, "status");
    }

    public static void LenientEngagementManifestRejectsMovesFromRetired()
    {
        var item = LenientEngagementManifest.NewLenientEngagementManifest("code-891", "label-522");
        item.Status = LenientEngagementManifestStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientEngagementManifestStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientEngagementManifestStartsInCreated();
        LenientEngagementManifestMovesToValidated();
        LenientEngagementManifestRejectsMovesFromRetired();
    }
}
