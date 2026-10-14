namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilitySegmentChecks
{
    public static void MobilitySegmentStartsInRequested()
    {
        var item = MobilitySegment.NewMobilitySegment("code-666", "name-811");
        Check.Equal(item.Status, MobilitySegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilitySegmentMovesToGranted()
    {
        var item = MobilitySegment.NewMobilitySegment("code-666", "name-811");
        try
        {
            item.MoveTo(MobilitySegmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilitySegmentStatus.Granted, "status");
    }

    public static void MobilitySegmentRejectsMovesFromExpired()
    {
        var item = MobilitySegment.NewMobilitySegment("code-666", "name-811");
        item.Status = MobilitySegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(MobilitySegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        MobilitySegmentStartsInRequested();
        MobilitySegmentMovesToGranted();
        MobilitySegmentRejectsMovesFromExpired();
    }
}
