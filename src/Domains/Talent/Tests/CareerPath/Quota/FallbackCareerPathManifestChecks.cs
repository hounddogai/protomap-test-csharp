namespace Acme.Workforce.Talent.Tests.CareerPath.Quota;

using Acme.Workforce.Talent.CareerPath.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FallbackCareerPathManifestChecks
{
    public static void FallbackCareerPathManifestStartsInDraft()
    {
        var item = FallbackCareerPathManifest.NewFallbackCareerPathManifest("code-978", "title-629");
        Check.Equal(item.Status, FallbackCareerPathManifestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackCareerPathManifestMovesToSuspended()
    {
        var item = FallbackCareerPathManifest.NewFallbackCareerPathManifest("code-978", "title-629");
        try
        {
            item.MoveTo(FallbackCareerPathManifestStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackCareerPathManifestStatus.Suspended, "status");
    }

    public static void FallbackCareerPathManifestRejectsMovesFromClosed()
    {
        var item = FallbackCareerPathManifest.NewFallbackCareerPathManifest("code-978", "title-629");
        item.Status = FallbackCareerPathManifestStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackCareerPathManifestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackCareerPathManifestStartsInDraft();
        FallbackCareerPathManifestMovesToSuspended();
        FallbackCareerPathManifestRejectsMovesFromClosed();
    }
}
