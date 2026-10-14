namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityManifestChecks
{
    public static void MobilityManifestStartsInCreated()
    {
        var item = MobilityManifest.NewMobilityManifest("code-55", "name-247");
        Check.Equal(item.Status, MobilityManifestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityManifestMovesToRetired()
    {
        var item = MobilityManifest.NewMobilityManifest("code-55", "name-247");
        try
        {
            item.MoveTo(MobilityManifestStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityManifestStatus.Retired, "status");
    }

    public static void MobilityManifestRejectsMovesFromRetired()
    {
        var item = MobilityManifest.NewMobilityManifest("code-55", "name-247");
        item.Status = MobilityManifestStatus.Retired;
        Check.Fails(() => item.MoveTo(MobilityManifestStatus.Created), "move");
    }

    public static void RunAll()
    {
        MobilityManifestStartsInCreated();
        MobilityManifestMovesToRetired();
        MobilityManifestRejectsMovesFromRetired();
    }
}
