namespace Acme.Workforce.Talent.Tests.CareerPath.OverrideItem;

using Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CappedCareerPathManifestChecks
{
    public static void CappedCareerPathManifestStartsInRequested()
    {
        var item = CappedCareerPathManifest.NewCappedCareerPathManifest("code-324", "name-575");
        Check.Equal(item.Status, CappedCareerPathManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedCareerPathManifestMovesToGranted()
    {
        var item = CappedCareerPathManifest.NewCappedCareerPathManifest("code-324", "name-575");
        try
        {
            item.MoveTo(CappedCareerPathManifestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedCareerPathManifestStatus.Granted, "status");
    }

    public static void CappedCareerPathManifestRejectsMovesFromExpired()
    {
        var item = CappedCareerPathManifest.NewCappedCareerPathManifest("code-324", "name-575");
        item.Status = CappedCareerPathManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedCareerPathManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedCareerPathManifestStartsInRequested();
        CappedCareerPathManifestMovesToGranted();
        CappedCareerPathManifestRejectsMovesFromExpired();
    }
}
