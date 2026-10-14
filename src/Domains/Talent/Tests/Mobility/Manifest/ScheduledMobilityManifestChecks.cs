namespace Acme.Workforce.Talent.Tests.Mobility.Manifest;

using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledMobilityManifestChecks
{
    public static void ScheduledMobilityManifestStartsInRequested()
    {
        var item = ScheduledMobilityManifest.NewScheduledMobilityManifest("code-764", "label-798");
        Check.Equal(item.Status, ScheduledMobilityManifestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledMobilityManifestMovesToGranted()
    {
        var item = ScheduledMobilityManifest.NewScheduledMobilityManifest("code-764", "label-798");
        try
        {
            item.MoveTo(ScheduledMobilityManifestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledMobilityManifestStatus.Granted, "status");
    }

    public static void ScheduledMobilityManifestRejectsMovesFromExpired()
    {
        var item = ScheduledMobilityManifest.NewScheduledMobilityManifest("code-764", "label-798");
        item.Status = ScheduledMobilityManifestStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledMobilityManifestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledMobilityManifestStartsInRequested();
        ScheduledMobilityManifestMovesToGranted();
        ScheduledMobilityManifestRejectsMovesFromExpired();
    }
}
