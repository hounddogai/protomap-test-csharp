namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class ScheduledPayBandManifestChecks
{
    public static void ScheduledPayBandManifestStartsInProposed()
    {
        var item = ScheduledPayBandManifest.NewScheduledPayBandManifest("code-387", "title-439");
        Check.Equal(item.Status, ScheduledPayBandManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayBandManifestMovesToApproved()
    {
        var item = ScheduledPayBandManifest.NewScheduledPayBandManifest("code-387", "title-439");
        try
        {
            item.MoveTo(ScheduledPayBandManifestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayBandManifestStatus.Approved, "status");
    }

    public static void ScheduledPayBandManifestRejectsMovesFromWithdrawn()
    {
        var item = ScheduledPayBandManifest.NewScheduledPayBandManifest("code-387", "title-439");
        item.Status = ScheduledPayBandManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledPayBandManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledPayBandManifestStartsInProposed();
        ScheduledPayBandManifestMovesToApproved();
        ScheduledPayBandManifestRejectsMovesFromWithdrawn();
    }
}
